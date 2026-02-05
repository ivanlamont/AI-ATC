using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Models.Navigation;

/// <summary>
/// Represents a flight route through multiple fixes
/// </summary>
public class Route
{
    /// <summary>
    /// Ordered list of fixes along the route
    /// </summary>
    public List<RouteSegment> Segments { get; set; } = new();

    /// <summary>
    /// Total route distance in nautical miles
    /// </summary>
    public float TotalDistanceNm
    {
        get => Segments.Sum(s => s.DistanceNm);
    }

    /// <summary>
    /// Adds a fix to the end of the route
    /// </summary>
    public void AddFix(Fix fix)
    {
        if (Segments.Count == 0)
        {
            Segments.Add(new RouteSegment
            {
                Fix = fix,
                DistanceNm = 0,
                CourseDegrees = 0
            });
        }
        else
        {
            var lastFix = Segments[^1].Fix;
            var distance = lastFix.GetDistanceNm(fix.PositionNm);
            var course = lastFix.GetBearingTo(fix.PositionNm);

            Segments.Add(new RouteSegment
            {
                Fix = fix,
                DistanceNm = distance,
                CourseDegrees = course
            });
        }
    }

    /// <summary>
    /// Gets the next fix after the current position along the route
    /// </summary>
    public Fix? GetNextFix(Vector2 currentPosition)
    {
        float minDistance = float.MaxValue;
        int closestIndex = -1;

        for (int i = 0; i < Segments.Count; i++)
        {
            var distance = Segments[i].Fix.GetDistanceNm(currentPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        // Return the next fix after the closest one
        if (closestIndex >= 0 && closestIndex + 1 < Segments.Count)
        {
            return Segments[closestIndex + 1].Fix;
        }

        return null;
    }

    /// <summary>
    /// Gets the course to fly from current position to next fix
    /// </summary>
    public float? GetCourseToNextFix(Vector2 currentPosition)
    {
        var nextFix = GetNextFix(currentPosition);
        if (nextFix == null)
            return null;

        var delta = nextFix.PositionNm - currentPosition;
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
/// Represents a segment of a route between two fixes
/// </summary>
public class RouteSegment
{
    /// <summary>
    /// The fix at the end of this segment
    /// </summary>
    public Fix Fix { get; set; } = null!;

    /// <summary>
    /// Distance to this fix from previous fix (NM)
    /// </summary>
    public float DistanceNm { get; set; }

    /// <summary>
    /// Course from previous fix to this fix (degrees)
    /// </summary>
    public float CourseDegrees { get; set; }

    /// <summary>
    /// Optional altitude constraint at this fix (feet)
    /// </summary>
    public float? AltitudeConstraintFt { get; set; }

    /// <summary>
    /// Optional speed constraint at this fix (knots)
    /// </summary>
    public float? SpeedConstraintKnots { get; set; }

    /// <summary>
    /// Type of altitude constraint
    /// </summary>
    public AltitudeConstraintType ConstraintType { get; set; } = AltitudeConstraintType.None;
}

/// <summary>
/// Type of altitude constraint at a fix
/// </summary>
public enum AltitudeConstraintType
{
    None,       // No constraint
    At,         // Exactly at altitude
    AtOrAbove,  // At or above altitude
    AtOrBelow   // At or below altitude
}
