using System;
using System.Collections.Generic;
using System.Linq;
using AIATC.Domain.Models.Commands;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Scoring;

/// <summary>
/// Service for calculating and tracking scores
/// </summary>
public class ScoringService
{
    private readonly Dictionary<string, AircraftHappiness> _aircraftHappiness = new();
    private SessionScore _currentSession = new();

    // Scoring constants
    private const int PointsSuccessfulLanding = 100;
    private const int PointsSuccessfulHandoff = 50;
    private const int PointsEfficientRoute = 25;
    private const int PointsProcedureCompliance = 15;

    private const int PenaltySeparationMinor = -25;
    private const int PenaltySeparationModerate = -75;
    private const int PenaltySeparationMajor = -150;
    private const int PenaltySeparationCritical = -300;

    private const int PenaltyAltitudeViolation = -20;
    private const int PenaltySpeedViolation = -15;
    private const int PenaltyDelayedClearance = -10;
    private const int PenaltyUnnecessaryCommand = -5;

    /// <summary>
    /// Gets the current session score
    /// </summary>
    public SessionScore GetCurrentSession() => _currentSession;

    /// <summary>
    /// Starts a new scoring session
    /// </summary>
    public void StartNewSession(string sessionId, float timeMultiplier = 1.0f)
    {
        _currentSession = new SessionScore
        {
            SessionId = sessionId,
            TimeMultiplier = timeMultiplier,
            StartTime = DateTime.UtcNow
        };
        _aircraftHappiness.Clear();
    }

    /// <summary>
    /// Ends the current session
    /// </summary>
    public void EndSession()
    {
        _currentSession.EndTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Registers a new aircraft in the session
    /// </summary>
    public void RegisterAircraft(string callsign, float directDistanceNm)
    {
        var happiness = new AircraftHappiness
        {
            Callsign = callsign,
            SpawnTime = DateTime.UtcNow,
            DirectDistance = directDistanceNm
        };

        _aircraftHappiness[callsign] = happiness;

        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.AircraftSpawned,
            Points = 0,
            AircraftCallsign = callsign,
            Description = $"Aircraft {callsign} entered airspace"
        });
    }

    /// <summary>
    /// Records a successful landing
    /// </summary>
    public void RecordLanding(string callsign)
    {
        if (!_aircraftHappiness.TryGetValue(callsign, out var happiness))
            return;

        happiness.LandedSuccessfully = true;

        // Award points based on aircraft happiness
        var happinessScore = happiness.GetFinalScore();

        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SuccessfulLanding,
            Points = PointsSuccessfulLanding + happinessScore,
            AircraftCallsign = callsign,
            Description = $"{callsign} landed successfully",
            Details = $"Aircraft happiness: {happiness.Happiness:F1}%, Efficiency: {happiness.GetRouteEfficiency():P1}"
        });
    }

    /// <summary>
    /// Records a successful handoff
    /// </summary>
    public void RecordHandoff(string callsign, string toFacility)
    {
        if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
        {
            var happinessBonus = (int)(happiness.Happiness / 10);

            _currentSession.AddEvent(new ScoreEvent
            {
                Type = ScoreEventType.SuccessfulHandoff,
                Points = PointsSuccessfulHandoff + happinessBonus,
                AircraftCallsign = callsign,
                Description = $"{callsign} handed off to {toFacility}",
                Details = $"Aircraft happiness: {happiness.Happiness:F1}%"
            });
        }
    }

    /// <summary>
    /// Records a command issued to an aircraft
    /// </summary>
    public void RecordCommand(string callsign, AtcCommand command)
    {
        if (!_aircraftHappiness.TryGetValue(callsign, out var happiness))
            return;

        happiness.CommandCount++;
        happiness.LastClearanceTime = DateTime.UtcNow;

        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.CommandIssued,
            Points = 0,
            AircraftCallsign = callsign,
            Description = $"Issued command to {callsign}",
            Details = command.GetReadback()
        });

        // Check if command seems unnecessary (too many commands)
        if (happiness.CommandCount > 10)
        {
            _currentSession.AddEvent(new ScoreEvent
            {
                Type = ScoreEventType.UnnecessaryCommand,
                Points = PenaltyUnnecessaryCommand,
                AircraftCallsign = callsign,
                Description = $"Excessive commands to {callsign}",
                Details = $"Total commands: {happiness.CommandCount}"
            });

            happiness.ModifyHappiness(-2, "Excessive vectoring");
        }
    }

    /// <summary>
    /// Records a separation violation
    /// </summary>
    public void RecordSeparationViolation(string callsign1, string callsign2, float separationNm)
    {
        // Determine severity based on separation distance
        var (severity, points) = GetSeparationPenalty(separationNm);

        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SeparationViolation,
            Points = points,
            Severity = severity,
            AircraftCallsign = callsign1,
            Description = $"Loss of separation: {callsign1} and {callsign2}",
            Details = $"Separation: {separationNm:F2} NM"
        });

        // Decrease happiness for both aircraft
        if (_aircraftHappiness.TryGetValue(callsign1, out var h1))
            h1.ModifyHappiness(-20, "Loss of separation");

        if (_aircraftHappiness.TryGetValue(callsign2, out var h2))
            h2.ModifyHappiness(-20, "Loss of separation");
    }

    /// <summary>
    /// Records an altitude constraint violation
    /// </summary>
    public void RecordAltitudeViolation(string callsign, float expectedAltitude, float actualAltitude)
    {
        var deviation = Math.Abs(expectedAltitude - actualAltitude);
        var severity = deviation > 1000 ? SeverityLevel.Major :
                      deviation > 500 ? SeverityLevel.Moderate : SeverityLevel.Minor;

        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.AltitudeViolation,
            Points = PenaltyAltitudeViolation,
            Severity = severity,
            AircraftCallsign = callsign,
            Description = $"{callsign} altitude constraint violation",
            Details = $"Expected: {expectedAltitude:F0} ft, Actual: {actualAltitude:F0} ft"
        });

        if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
            happiness.ModifyHappiness(-10, "Altitude constraint violation");
    }

    /// <summary>
    /// Records a speed constraint violation
    /// </summary>
    public void RecordSpeedViolation(string callsign, float expectedSpeed, float actualSpeed)
    {
        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.SpeedViolation,
            Points = PenaltySpeedViolation,
            Severity = SeverityLevel.Minor,
            AircraftCallsign = callsign,
            Description = $"{callsign} speed constraint violation",
            Details = $"Expected: {expectedSpeed:F0} kts, Actual: {actualSpeed:F0} kts"
        });

        if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
            happiness.ModifyHappiness(-5, "Speed constraint violation");
    }

    /// <summary>
    /// Records efficient routing
    /// </summary>
    public void RecordEfficientRoute(string callsign, float efficiency)
    {
        if (efficiency >= 0.9f)
        {
            _currentSession.AddEvent(new ScoreEvent
            {
                Type = ScoreEventType.EfficientRouting,
                Points = PointsEfficientRoute,
                AircraftCallsign = callsign,
                Description = $"Efficient routing for {callsign}",
                Details = $"Efficiency: {efficiency:P1}"
            });

            if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
                happiness.ModifyHappiness(5, "Efficient routing");
        }
    }

    /// <summary>
    /// Records procedure compliance
    /// </summary>
    public void RecordProcedureCompliance(string callsign, string procedureName)
    {
        _currentSession.AddEvent(new ScoreEvent
        {
            Type = ScoreEventType.ProcedureCompliance,
            Points = PointsProcedureCompliance,
            AircraftCallsign = callsign,
            Description = $"{callsign} followed {procedureName}",
            Details = "Procedure compliance"
        });

        if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
            happiness.ModifyHappiness(3, "Followed published procedure");
    }

    /// <summary>
    /// Updates aircraft distance tracking
    /// </summary>
    public void UpdateAircraftDistance(string callsign, float distanceFlown)
    {
        if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
        {
            happiness.TotalDistanceFlown = distanceFlown;
        }
    }

    /// <summary>
    /// Updates time in holding pattern
    /// </summary>
    public void UpdateHoldingTime(string callsign, float timeInHoldSeconds)
    {
        if (_aircraftHappiness.TryGetValue(callsign, out var happiness))
        {
            happiness.TimeInHold = timeInHoldSeconds;

            // Decrease happiness for extended holding
            if (timeInHoldSeconds > 300) // More than 5 minutes
            {
                var excessTime = (timeInHoldSeconds - 300) / 60;
                happiness.ModifyHappiness(-excessTime * 2, "Extended holding");
            }
        }
    }

    /// <summary>
    /// Gets aircraft happiness
    /// </summary>
    public AircraftHappiness? GetAircraftHappiness(string callsign)
    {
        return _aircraftHappiness.TryGetValue(callsign, out var happiness) ? happiness : null;
    }

    /// <summary>
    /// Gets all aircraft happiness values
    /// </summary>
    public Dictionary<string, AircraftHappiness> GetAllAircraftHappiness()
    {
        return new Dictionary<string, AircraftHappiness>(_aircraftHappiness);
    }

    private (SeverityLevel severity, int points) GetSeparationPenalty(float separationNm)
    {
        if (separationNm < 1.0f)
            return (SeverityLevel.Critical, PenaltySeparationCritical);
        else if (separationNm < 2.0f)
            return (SeverityLevel.Major, PenaltySeparationMajor);
        else if (separationNm < 2.5f)
            return (SeverityLevel.Moderate, PenaltySeparationModerate);
        else
            return (SeverityLevel.Minor, PenaltySeparationMinor);
    }
}
