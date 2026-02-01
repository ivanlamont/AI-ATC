using System;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Airspace;

/// <summary>
/// Represents an airspace sector with defined boundaries and control frequency
/// </summary>
public class Sector
{
    /// <summary>
    /// Unique sector identifier (e.g., "NCT_APP1", "SFO_TWR")
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the sector
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of airspace sector
    /// </summary>
    public SectorType Type { get; set; }

    /// <summary>
    /// Primary control frequency (MHz)
    /// </summary>
    public float FrequencyMhz { get; set; }

    /// <summary>
    /// Secondary/backup frequency (MHz)
    /// </summary>
    public float? SecondaryFrequencyMhz { get; set; }

    /// <summary>
    /// Sector boundary definition
    /// </summary>
    public SectorBoundary Boundary { get; set; } = new();

    /// <summary>
    /// Altitude limits for this sector
    /// </summary>
    public AltitudeLimit AltitudeLimits { get; set; } = new();

    /// <summary>
    /// Adjacent sectors for handoffs
    /// </summary>
    public List<string> AdjacentSectors { get; set; } = new();

    /// <summary>
    /// Controller callsign/position (e.g., "NorCal Approach", "San Francisco Tower")
    /// </summary>
    public string ControllerCallsign { get; set; } = string.Empty;

    /// <summary>
    /// Whether this sector is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Checks if a position is within this sector's lateral boundaries
    /// </summary>
    public bool ContainsPosition(Vector2 position)
    {
        return Boundary.ContainsPoint(position);
    }

    /// <summary>
    /// Checks if an aircraft is within this sector (lateral and vertical)
    /// </summary>
    public bool ContainsAircraft(AircraftModel aircraft)
    {
        var inLateralBounds = ContainsPosition(aircraft.PositionNm);
        var inAltitudeBounds = AltitudeLimits.ContainsAltitude(aircraft.AltitudeFt);

        return inLateralBounds && inAltitudeBounds;
    }

    /// <summary>
    /// Gets the distance from a position to the nearest sector boundary
    /// </summary>
    public float GetDistanceToBoundary(Vector2 position)
    {
        return Boundary.GetDistanceToBoundary(position);
    }
}

/// <summary>
/// Type of airspace sector
/// </summary>
public enum SectorType
{
    Tower,          // Airport tower control
    Approach,       // Terminal radar approach control (TRACON)
    Departure,      // Departure control
    Center,         // En-route center control
    Ground,         // Ground control at airport
    Clearance       // Clearance delivery
}

/// <summary>
/// Altitude limits for a sector
/// </summary>
public class AltitudeLimit
{
    /// <summary>
    /// Minimum altitude (feet MSL), null for surface
    /// </summary>
    public float? MinimumAltitude { get; set; }

    /// <summary>
    /// Maximum altitude (feet MSL), null for unlimited
    /// </summary>
    public float? MaximumAltitude { get; set; }

    /// <summary>
    /// Checks if an altitude is within these limits
    /// </summary>
    public bool ContainsAltitude(float altitudeFt)
    {
        if (MinimumAltitude.HasValue && altitudeFt < MinimumAltitude.Value)
            return false;

        if (MaximumAltitude.HasValue && altitudeFt > MaximumAltitude.Value)
            return false;

        return true;
    }
}

/// <summary>
/// Defines the lateral boundary of a sector
/// </summary>
public class SectorBoundary
{
    /// <summary>
    /// Vertices defining the polygon boundary (in NM coordinates)
    /// </summary>
    public List<Vector2> Vertices { get; set; } = new();

    /// <summary>
    /// Center point of the sector (for display purposes)
    /// </summary>
    public Vector2? Center { get; set; }

    /// <summary>
    /// Radius for circular boundaries (NM), null if polygon
    /// </summary>
    public float? RadiusNm { get; set; }

    /// <summary>
    /// Checks if a point is inside the boundary using ray casting algorithm
    /// </summary>
    public bool ContainsPoint(Vector2 point)
    {
        // If circular boundary
        if (RadiusNm.HasValue && Center.HasValue)
        {
            var distance = (point - Center.Value).Magnitude;
            return distance <= RadiusNm.Value;
        }

        // If polygon boundary, use ray casting
        if (Vertices.Count < 3)
            return false;

        bool inside = false;
        int j = Vertices.Count - 1;

        for (int i = 0; i < Vertices.Count; i++)
        {
            if (((Vertices[i].Y > point.Y) != (Vertices[j].Y > point.Y)) &&
                (point.X < (Vertices[j].X - Vertices[i].X) * (point.Y - Vertices[i].Y) /
                (Vertices[j].Y - Vertices[i].Y) + Vertices[i].X))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    /// <summary>
    /// Gets the distance from a point to the nearest boundary edge
    /// </summary>
    public float GetDistanceToBoundary(Vector2 point)
    {
        // If circular boundary
        if (RadiusNm.HasValue && Center.HasValue)
        {
            var distance = (point - Center.Value).Magnitude;
            return Math.Abs(distance - RadiusNm.Value);
        }

        // If polygon, find minimum distance to any edge
        float minDistance = float.MaxValue;

        for (int i = 0; i < Vertices.Count; i++)
        {
            int j = (i + 1) % Vertices.Count;
            var distance = DistanceToLineSegment(point, Vertices[i], Vertices[j]);
            minDistance = Math.Min(minDistance, distance);
        }

        return minDistance;
    }

    private float DistanceToLineSegment(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        var lineVec = lineEnd - lineStart;
        var pointVec = point - lineStart;

        var lineLengthSq = lineVec.X * lineVec.X + lineVec.Y * lineVec.Y;

        if (lineLengthSq == 0)
            return pointVec.Magnitude;

        var t = Math.Max(0, Math.Min(1,
            (pointVec.X * lineVec.X + pointVec.Y * lineVec.Y) / lineLengthSq));

        var projection = lineStart + new Vector2(lineVec.X * t, lineVec.Y * t);
        return (point - projection).Magnitude;
    }
}
