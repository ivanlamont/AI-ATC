using System;

namespace AIATC.Domain.Models.Scoring;

/// <summary>
/// Represents a scoring event that affects the player's score
/// </summary>
public class ScoreEvent
{
    /// <summary>
    /// Unique identifier for this event
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Type of scoring event
    /// </summary>
    public ScoreEventType Type { get; set; }

    /// <summary>
    /// Points awarded (positive) or deducted (negative)
    /// </summary>
    public int Points { get; set; }

    /// <summary>
    /// Timestamp of the event
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Aircraft callsign involved in the event (if applicable)
    /// </summary>
    public string? AircraftCallsign { get; set; }

    /// <summary>
    /// Description of the event for display
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Additional details about the event
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Severity level (for violations)
    /// </summary>
    public SeverityLevel Severity { get; set; } = SeverityLevel.None;
}

/// <summary>
/// Types of scoring events
/// </summary>
public enum ScoreEventType
{
    // Positive events
    SuccessfulLanding,
    SuccessfulHandoff,
    EfficientRouting,
    ProcedureCompliance,
    TimeBonus,

    // Negative events
    SeparationViolation,
    AltitudeViolation,
    SpeedViolation,
    RouteDeviation,
    DelayedClearance,
    UnnecessaryCommand,

    // Neutral events
    CommandIssued,
    AircraftSpawned,
    AircraftHandedOff
}

/// <summary>
/// Severity level for violations
/// </summary>
public enum SeverityLevel
{
    None,       // Not a violation
    Minor,      // Small deviation, minor penalty
    Moderate,   // Significant deviation, moderate penalty
    Major,      // Serious violation, major penalty
    Critical    // Dangerous situation, severe penalty
}
