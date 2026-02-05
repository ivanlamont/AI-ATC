using System;
using AIATC.Domain.Models.Commands;

namespace AIATC.Domain.Models.Navigation;

/// <summary>
/// Service for integrating navigation with aircraft guidance
/// </summary>
public class NavigationService
{
    private readonly NavigationDatabase _database;

    public NavigationService(NavigationDatabase database)
    {
        _database = database;
    }

    /// <summary>
    /// Processes a DirectCommand and calculates heading to the fix
    /// </summary>
    public (bool success, float? heading, string? error) ProcessDirectCommand(
        DirectCommand command,
        AircraftModel aircraft)
    {
        var fix = _database.GetFix(command.FixName);
        if (fix == null)
        {
            return (false, null, $"Fix {command.FixName} not found in database");
        }

        var delta = fix.PositionNm - aircraft.PositionNm;
        var angleRad = MathF.Atan2(delta.Y, delta.X);

        // Convert from trigonometric to aviation heading
        var headingDeg = 90 - (angleRad * SimulationConstants.RadiansToDegrees);

        // Normalize to 0-360
        while (headingDeg < 0) headingDeg += 360;
        while (headingDeg >= 360) headingDeg -= 360;

        return (true, headingDeg, null);
    }

    /// <summary>
    /// Gets the procedure for an approach command
    /// </summary>
    public (bool success, Procedure? procedure, string? error) ProcessApproachCommand(
        ApproachCommand command,
        string airportIdentifier)
    {
        var approaches = _database.GetApproachesForRunway(airportIdentifier, command.RunwayIdentifier);

        // Find matching approach type
        var procedure = approaches.Find(a =>
            a.Identifier.Contains(command.Type.ToString(), StringComparison.OrdinalIgnoreCase));

        if (procedure == null)
        {
            return (false, null,
                $"No {command.Type} approach found for runway {command.RunwayIdentifier}");
        }

        return (true, procedure, null);
    }

    /// <summary>
    /// Calculates hold entry and holding pattern
    /// </summary>
    public (bool success, HoldingPattern? pattern, string? error) ProcessHoldCommand(
        HoldCommand command,
        AircraftModel aircraft)
    {
        var fix = _database.GetFix(command.FixName);
        if (fix == null)
        {
            return (false, null, $"Fix {command.FixName} not found in database");
        }

        // Determine inbound course
        float inboundCourse = command.InboundCourseDegrees ?? 360.0f;

        // Calculate hold entry type based on aircraft position
        var bearingToFix = CalculateBearing(aircraft.PositionNm, fix.PositionNm);
        var entryType = DetermineHoldEntry(
            aircraft.HeadingDegrees,
            bearingToFix,
            inboundCourse,
            command.TurnDirection);

        var pattern = new HoldingPattern
        {
            Fix = fix,
            InboundCourseDegrees = inboundCourse,
            TurnDirection = command.TurnDirection,
            EntryType = entryType
        };

        return (true, pattern, null);
    }

    /// <summary>
    /// Updates aircraft autopilot to follow a route
    /// </summary>
    public void FollowRoute(AircraftModel aircraft, Route route)
    {
        var nextFix = route.GetNextFix(aircraft.PositionNm);
        if (nextFix == null)
            return;

        // Calculate heading to next fix
        var delta = nextFix.PositionNm - aircraft.PositionNm;
        var angleRad = MathF.Atan2(delta.Y, delta.X);

        // Convert from trigonometric to aviation heading
        var headingDeg = 90 - (angleRad * SimulationConstants.RadiansToDegrees);

        // Normalize to 0-360
        while (headingDeg < 0) headingDeg += 360;
        while (headingDeg >= 360) headingDeg -= 360;

        // Set target heading
        aircraft.TargetHeadingDegrees = headingDeg;

        // Calculate turn rate
        var headingDiff = headingDeg - aircraft.HeadingDegrees;
        while (headingDiff > 180) headingDiff -= 360;
        while (headingDiff < -180) headingDiff += 360;

        var turnRateDegPerSec = Math.Clamp(headingDiff * 0.5f,
            -SimulationConstants.MaxTurnRate,
            SimulationConstants.MaxTurnRate);

        aircraft.TurnRateRadPerSec = turnRateDegPerSec * SimulationConstants.DegreesToRadians;
    }

    private float CalculateBearing(Vector2 from, Vector2 to)
    {
        var delta = to - from;
        var angleRad = MathF.Atan2(delta.Y, delta.X);

        // Convert from trigonometric to aviation heading
        var headingDeg = 90 - (angleRad * SimulationConstants.RadiansToDegrees);

        // Normalize to 0-360
        while (headingDeg < 0) headingDeg += 360;
        while (headingDeg >= 360) headingDeg -= 360;

        return headingDeg;
    }

    private HoldEntryType DetermineHoldEntry(
        float aircraftHeading,
        float bearingToFix,
        float inboundCourse,
        TurnDirection turnDirection)
    {
        // Calculate angle between aircraft heading and inbound course
        var angle = aircraftHeading - inboundCourse;
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;

        // Standard right-hand hold entry determination
        if (turnDirection == TurnDirection.Right)
        {
            if (angle >= -70 && angle <= 110)
                return HoldEntryType.Direct;
            else if (angle > 110 && angle <= 180)
                return HoldEntryType.Parallel;
            else
                return HoldEntryType.Teardrop;
        }
        else // Left-hand holds
        {
            if (angle >= -110 && angle <= 70)
                return HoldEntryType.Direct;
            else if (angle < -110 && angle >= -180)
                return HoldEntryType.Parallel;
            else
                return HoldEntryType.Teardrop;
        }
    }
}

/// <summary>
/// Represents a holding pattern at a fix
/// </summary>
public class HoldingPattern
{
    public Fix Fix { get; set; } = null!;
    public float InboundCourseDegrees { get; set; }
    public TurnDirection TurnDirection { get; set; }
    public HoldEntryType EntryType { get; set; }
    public float LegLengthMinutes { get; set; } = 1.0f; // Standard 1-minute legs
}

/// <summary>
/// Type of holding pattern entry
/// </summary>
public enum HoldEntryType
{
    Direct,     // Fly directly to fix and enter hold
    Parallel,   // Parallel entry (offset track)
    Teardrop    // Teardrop entry (30-degree turn)
}
