namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Aircraft type with performance characteristics and operational parameters
/// </summary>
public class AircraftType
{
    /// <summary>
    /// ICAO aircraft type designator (e.g., B738, A320, E145)
    /// </summary>
    public string IcaoCode { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturer and model name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Aircraft category for separation standards
    /// </summary>
    public AircraftCategory Category { get; set; }

    /// <summary>
    /// Wake turbulence category
    /// </summary>
    public WakeCategory WakeCategory { get; set; }

    /// <summary>
    /// Maximum takeoff weight in pounds
    /// </summary>
    public int MaxTakeoffWeightLbs { get; set; }

    /// <summary>
    /// Maximum certified altitude in feet
    /// </summary>
    public int ServiceCeilingFt { get; set; }

    /// <summary>
    /// Reference approach speed (Vref) in knots
    /// </summary>
    public int VrefSpeedKnots { get; set; }

    /// <summary>
    /// Minimum approach speed in knots
    /// </summary>
    public int MinApproachSpeedKnots { get; set; }

    /// <summary>
    /// Maximum cruise speed in knots
    /// </summary>
    public int MaxCruiseSpeedKnots { get; set; }

    /// <summary>
    /// Typical cruise speed in knots
    /// </summary>
    public int TypicalCruiseSpeedKnots { get; set; }

    /// <summary>
    /// Maximum climb rate in feet per minute
    /// </summary>
    public int MaxClimbRateFpm { get; set; }

    /// <summary>
    /// Maximum descent rate in feet per minute
    /// </summary>
    public int MaxDescentRateFpm { get; set; }

    /// <summary>
    /// Standard turn rate in degrees per second
    /// </summary>
    public float StandardTurnRate { get; set; } = 3.0f;

    /// <summary>
    /// Engine type
    /// </summary>
    public EngineType EngineType { get; set; }

    /// <summary>
    /// Number of engines
    /// </summary>
    public int NumberOfEngines { get; set; }

    /// <summary>
    /// Fuel consumption rate in gallons per hour (cruise)
    /// </summary>
    public int FuelConsumptionGph { get; set; }
}

/// <summary>
/// Aircraft categories for ATC separation and approach procedures
/// </summary>
public enum AircraftCategory
{
    A, // Light single-engine
    B, // Light twin-engine 
    C, // Large aircraft
    D, // High-performance large aircraft
    E  // Heavy aircraft
}

/// <summary>
/// Wake turbulence categories
/// </summary>
public enum WakeCategory
{
    Light,    // < 41,000 lbs
    Medium,   // 41,000 - 300,000 lbs
    Heavy,    // > 300,000 lbs
    Super     // A380 and similar super-heavy aircraft
}

/// <summary>
/// Engine types
/// </summary>
public enum EngineType
{
    Piston,
    Turboprop,
    Turbojet,
    Turbofan
}