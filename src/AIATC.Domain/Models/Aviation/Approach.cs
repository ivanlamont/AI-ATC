using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Represents a published approach procedure
/// </summary>
public class Approach
{
    /// <summary>
    /// Approach identifier (e.g., "ILS Z RWY 28L", "RNAV (GPS) Y RWY 01L")
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Airport this approach serves
    /// </summary>
    public string AirportIcaoCode { get; set; } = string.Empty;

    /// <summary>
    /// Runway this approach serves
    /// </summary>
    public string RunwayIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Approach type
    /// </summary>
    public ApproachType Type { get; set; }

    /// <summary>
    /// Approach suffix (e.g., X, Y, Z for multiple approaches to same runway)
    /// </summary>
    public string? Suffix { get; set; }

    /// <summary>
    /// Final approach course in degrees magnetic
    /// </summary>
    public float FinalApproachCourse { get; set; }

    /// <summary>
    /// Final approach fix (FAF)
    /// </summary>
    public string FinalApproachFix { get; set; } = string.Empty;

    /// <summary>
    /// Missed approach point
    /// </summary>
    public string MissedApproachPoint { get; set; } = string.Empty;

    /// <summary>
    /// Decision altitude/height in feet
    /// </summary>
    public int? DecisionAltitudeFt { get; set; }

    /// <summary>
    /// Minimum descent altitude in feet
    /// </summary>
    public int? MinimumDescentAltitudeFt { get; set; }

    /// <summary>
    /// Approach waypoints in sequence
    /// </summary>
    public List<ApproachWaypoint> Waypoints { get; set; } = new();

    /// <summary>
    /// ILS frequency for precision approaches
    /// </summary>
    public float? IlsFrequency { get; set; }

    /// <summary>
    /// Glideslope angle in degrees
    /// </summary>
    public float GlideslopeAngle { get; set; } = 3.0f;

    /// <summary>
    /// Whether this is a precision approach
    /// </summary>
    public bool IsPrecisionApproach => Type == ApproachType.ILS || Type == ApproachType.GLS;
}

/// <summary>
/// Approach waypoint with specific instructions
/// </summary>
public class ApproachWaypoint
{
    public string FixName { get; set; } = string.Empty;
    public Vector2 PositionNm { get; set; }
    public int? AltitudeRestrictionFt { get; set; }
    public int? SpeedRestrictionKnots { get; set; }
    public WaypointType Type { get; set; }
}

/// <summary>
/// Types of approach procedures
/// </summary>
public enum ApproachType
{
    ILS,        // Instrument Landing System
    RNAV,       // Area Navigation (GPS)
    VOR,        // VHF Omnidirectional Range
    NDB,        // Non-Directional Beacon
    LOC,        // Localizer
    LDA,        // Localizer Type Directional Aid
    SDF,        // Simplified Directional Facility
    GPS,        // Global Positioning System
    GLS,        // GNSS Landing System
    Visual,     // Visual Approach
    Contact,    // Contact Approach
    Circling    // Circle-to-Land
}

/// <summary>
/// Waypoint types in approach procedures
/// </summary>
public enum WaypointType
{
    Initial,
    Intermediate, 
    Final,
    Missed,
    StepDown
}