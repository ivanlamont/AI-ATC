using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Models.Airspace;

/// <summary>
/// Manages aircraft handoffs between sectors
/// </summary>
public class HandoffManager
{
    private readonly Dictionary<string, Sector> _sectors = new();
    private readonly Dictionary<string, string> _aircraftSectorAssignments = new();
    private readonly Dictionary<string, HandoffState> _pendingHandoffs = new();

    /// <summary>
    /// Adds a sector to the manager
    /// </summary>
    public void AddSector(Sector sector)
    {
        _sectors[sector.Identifier] = sector;
    }

    /// <summary>
    /// Gets a sector by identifier
    /// </summary>
    public Sector? GetSector(string identifier)
    {
        return _sectors.TryGetValue(identifier, out var sector) ? sector : null;
    }

    /// <summary>
    /// Gets all sectors
    /// </summary>
    public List<Sector> GetAllSectors()
    {
        return _sectors.Values.ToList();
    }

    /// <summary>
    /// Assigns an aircraft to a sector
    /// </summary>
    public void AssignAircraftToSector(string callsign, string sectorId)
    {
        _aircraftSectorAssignments[callsign] = sectorId;
    }

    /// <summary>
    /// Gets the sector an aircraft is assigned to
    /// </summary>
    public Sector? GetAircraftSector(string callsign)
    {
        if (_aircraftSectorAssignments.TryGetValue(callsign, out var sectorId))
        {
            return GetSector(sectorId);
        }
        return null;
    }

    /// <summary>
    /// Checks if an aircraft needs a handoff based on its position
    /// </summary>
    public HandoffRecommendation? CheckHandoffNeeded(AircraftModel aircraft)
    {
        var currentSector = GetAircraftSector(aircraft.Callsign);
        if (currentSector == null)
            return null;

        // Check if still in current sector
        if (currentSector.ContainsAircraft(aircraft))
        {
            // Check proximity to boundary
            var distanceToBoundary = currentSector.GetDistanceToBoundary(aircraft.PositionNm);

            if (distanceToBoundary < 5.0f) // Within 5 NM of boundary
            {
                var targetSector = FindTargetSector(aircraft, currentSector);
                if (targetSector != null)
                {
                    return new HandoffRecommendation
                    {
                        Callsign = aircraft.Callsign,
                        CurrentSector = currentSector,
                        TargetSector = targetSector,
                        DistanceToBoundary = distanceToBoundary,
                        Urgency = distanceToBoundary < 2.0f ? HandoffUrgency.Urgent : HandoffUrgency.Normal
                    };
                }
            }

            return null;
        }

        // Aircraft has left current sector
        var newSector = FindSectorContainingAircraft(aircraft);
        if (newSector != null && newSector.Identifier != currentSector.Identifier)
        {
            return new HandoffRecommendation
            {
                Callsign = aircraft.Callsign,
                CurrentSector = currentSector,
                TargetSector = newSector,
                DistanceToBoundary = 0,
                Urgency = HandoffUrgency.Immediate // Aircraft has crossed boundary
            };
        }

        return null;
    }

    /// <summary>
    /// Initiates a handoff
    /// </summary>
    public void InitiateHandoff(string callsign, string targetSectorId)
    {
        var currentSector = GetAircraftSector(callsign);
        var targetSector = GetSector(targetSectorId);

        if (currentSector == null || targetSector == null)
            return;

        _pendingHandoffs[callsign] = new HandoffState
        {
            Callsign = callsign,
            FromSector = currentSector,
            ToSector = targetSector,
            InitiatedTime = DateTime.UtcNow,
            Status = HandoffStatus.Initiated
        };
    }

    /// <summary>
    /// Accepts a pending handoff
    /// </summary>
    public bool AcceptHandoff(string callsign)
    {
        if (!_pendingHandoffs.TryGetValue(callsign, out var handoff))
            return false;

        handoff.Status = HandoffStatus.Accepted;
        handoff.AcceptedTime = DateTime.UtcNow;

        // Complete the handoff
        CompleteHandoff(callsign);
        return true;
    }

    /// <summary>
    /// Completes a handoff
    /// </summary>
    public void CompleteHandoff(string callsign)
    {
        if (!_pendingHandoffs.TryGetValue(callsign, out var handoff))
            return;

        // Reassign to new sector
        _aircraftSectorAssignments[callsign] = handoff.ToSector.Identifier;

        handoff.Status = HandoffStatus.Completed;
        handoff.CompletedTime = DateTime.UtcNow;

        _pendingHandoffs.Remove(callsign);
    }

    /// <summary>
    /// Gets pending handoff for an aircraft
    /// </summary>
    public HandoffState? GetPendingHandoff(string callsign)
    {
        return _pendingHandoffs.TryGetValue(callsign, out var handoff) ? handoff : null;
    }

    /// <summary>
    /// Gets all pending handoffs
    /// </summary>
    public List<HandoffState> GetAllPendingHandoffs()
    {
        return _pendingHandoffs.Values.ToList();
    }

    /// <summary>
    /// Automatically assigns aircraft to appropriate sector based on position
    /// </summary>
    public void AutoAssignSector(AircraftModel aircraft)
    {
        var sector = FindSectorContainingAircraft(aircraft);
        if (sector != null)
        {
            AssignAircraftToSector(aircraft.Callsign, sector.Identifier);
        }
    }

    private Sector? FindSectorContainingAircraft(AircraftModel aircraft)
    {
        return _sectors.Values.FirstOrDefault(s => s.ContainsAircraft(aircraft));
    }

    private Sector? FindTargetSector(AircraftModel aircraft, Sector currentSector)
    {
        // Check adjacent sectors first
        foreach (var adjacentId in currentSector.AdjacentSectors)
        {
            var sector = GetSector(adjacentId);
            if (sector != null && sector.IsActive)
            {
                // Simple prediction: check if aircraft is heading towards this sector
                var sectorCenter = sector.Boundary.Center;
                if (!sectorCenter.HasValue)
                    continue;

                var vectorToSector = sectorCenter.Value - aircraft.PositionNm;
                var angleToSector = MathF.Atan2(vectorToSector.Y, vectorToSector.X);

                // Convert aircraft heading from aviation (0=North) to trig (0=East)
                var headingRad = (90 - aircraft.HeadingDegrees) * SimulationConstants.DegreesToRadians;

                // Normalize both angles to 0-2π
                while (angleToSector < 0) angleToSector += 2 * MathF.PI;
                while (headingRad < 0) headingRad += 2 * MathF.PI;

                // Check if aircraft is roughly heading towards sector (within 90 degrees)
                var angleDiff = MathF.Abs(angleToSector - headingRad);
                if (angleDiff > MathF.PI) angleDiff = 2 * MathF.PI - angleDiff;

                if (angleDiff < MathF.PI / 2) // Within 90 degrees
                {
                    return sector;
                }
            }
        }

        return null;
    }
}

/// <summary>
/// Recommendation for a handoff
/// </summary>
public class HandoffRecommendation
{
    public string Callsign { get; set; } = string.Empty;
    public Sector CurrentSector { get; set; } = null!;
    public Sector TargetSector { get; set; } = null!;
    public float DistanceToBoundary { get; set; }
    public HandoffUrgency Urgency { get; set; }
}

/// <summary>
/// State of a handoff in progress
/// </summary>
public class HandoffState
{
    public string Callsign { get; set; } = string.Empty;
    public Sector FromSector { get; set; } = null!;
    public Sector ToSector { get; set; } = null!;
    public DateTime InitiatedTime { get; set; }
    public DateTime? AcceptedTime { get; set; }
    public DateTime? CompletedTime { get; set; }
    public HandoffStatus Status { get; set; }
}

/// <summary>
/// Urgency level for handoffs
/// </summary>
public enum HandoffUrgency
{
    Normal,     // Plenty of time before boundary
    Urgent,     // Close to boundary (< 2 NM)
    Immediate   // Has crossed boundary
}

/// <summary>
/// Status of a handoff
/// </summary>
public enum HandoffStatus
{
    Initiated,  // Handoff offered
    Accepted,   // Receiving sector accepted
    Completed,  // Aircraft switched frequency
    Rejected    // Receiving sector rejected (rare)
}
