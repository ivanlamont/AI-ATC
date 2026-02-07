using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Represents a runway with physical characteristics and operational data
/// </summary>
public class Runway
{
    /// <summary>
    /// Runway identifier (e.g., "04L", "22R")
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Airport this runway belongs to
    /// </summary>
    public string AirportIcaoCode { get; set; } = string.Empty;

    /// <summary>
    /// Runway length in feet
    /// </summary>
    public int LengthFt { get; set; }

    /// <summary>
    /// Runway length alias for Entity Framework
    /// </summary>
    public int Length => LengthFt;

    /// <summary>
    /// Runway width in feet
    /// </summary>
    public int WidthFt { get; set; }

    /// <summary>
    /// Runway width alias for Entity Framework
    /// </summary>
    public int Width => WidthFt;

    /// <summary>
    /// Surface type (ASPH, CONC, etc.)
    /// </summary>
    public string SurfaceType { get; set; } = "ASPH";

    /// <summary>
    /// Magnetic bearing of runway in degrees
    /// </summary>
    public float MagneticBearing { get; set; }

    /// <summary>
    /// Runway gradient as percentage
    /// </summary>
    public float GradientPercent { get; set; }

    /// <summary>
    /// Surface type (e.g., Asphalt, Concrete, Grass)
    /// </summary>
    public RunwaySurface Surface { get; set; } = RunwaySurface.Asphalt;

    /// <summary>
    /// Threshold position (start of runway)
    /// </summary>
    public Vector2 ThresholdPositionNm { get; set; }

    /// <summary>
    /// End position (end of runway)
    /// </summary>
    public Vector2 EndPositionNm { get; set; }

    /// <summary>
    /// Runway elevation at threshold in feet MSL
    /// </summary>
    public float ThresholdElevationFt { get; set; }

    /// <summary>
    /// ILS localizer frequency if equipped
    /// </summary>
    public float? LocalizerFrequency { get; set; }

    /// <summary>
    /// ILS localizer course
    /// </summary>
    public float? LocalizerCourse { get; set; }

    /// <summary>
    /// Whether runway has precision approach capability
    /// </summary>
    public bool HasPrecisionApproach { get; set; }

    /// <summary>
    /// Calculate center point of runway
    /// </summary>
    public Vector2 CenterPositionNm => (ThresholdPositionNm + EndPositionNm) * 0.5f;
}

/// <summary>
/// Runway surface types
/// </summary>
public enum RunwaySurface
{
    Asphalt,
    Concrete,
    Grass,
    Gravel,
    Water,
    Ice,
    Snow
}