using System;

namespace AIATC.Domain.Models.Navigation;

/// <summary>
/// Represents a navigation fix/waypoint
/// </summary>
public class Fix
{
    /// <summary>
    /// 5-letter fix identifier (e.g., BEBOP, SUNST, DUMBA)
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Position in local NM coordinates
    /// </summary>
    public Vector2 PositionNm { get; set; }

    /// <summary>
    /// Latitude (for display/export)
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude (for display/export)
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Type of fix (VOR, NDB, GPS, etc.)
    /// </summary>
    public FixType Type { get; set; }

    /// <summary>
    /// Description/name
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Calculates distance from this fix to a position
    /// </summary>
    public float GetDistanceNm(Vector2 position)
    {
        return (position - PositionNm).Magnitude;
    }

    /// <summary>
    /// Calculates bearing from this fix to a position (degrees)
    /// </summary>
    public float GetBearingTo(Vector2 position)
    {
        var delta = position - PositionNm;
        var angleRad = MathF.Atan2(delta.Y, delta.X);

        // Convert from trigonometric to aviation heading
        var headingDeg = 90 - (angleRad * SimulationConstants.RadiansToDegrees);

        // Normalize to 0-360
        while (headingDeg < 0) headingDeg += 360;
        while (headingDeg >= 360) headingDeg -= 360;

        return headingDeg;
    }
}

/// <summary>
/// Type of navigation fix
/// </summary>
public enum FixType
{
    GPS,        // GPS waypoint
    VOR,        // VHF Omnidirectional Range
    NDB,        // Non-Directional Beacon
    DME,        // Distance Measuring Equipment
    Intersection // Intersection of radials
}
