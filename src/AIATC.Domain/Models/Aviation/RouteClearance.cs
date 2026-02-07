using System.Collections;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Represents an ATC route clearance as a collection of navigation legs
/// </summary>
public class RouteClearance : ICollection<ClearanceLeg>
{
    private readonly List<ClearanceLeg> _legs = new();

    /// <summary>
    /// Current active leg (first in the sequence)
    /// </summary>
    public ClearanceLeg? CurrentLeg => _legs.FirstOrDefault();

    /// <summary>
    /// Remaining legs to complete
    /// </summary>
    public IReadOnlyList<ClearanceLeg> RemainingLegs => _legs.AsReadOnly();

    /// <summary>
    /// Check if clearance contains a specific fix
    /// </summary>
    public bool ContainsFix(string fixIdentifier)
    {
        return _legs.Any(leg => leg.TargetIdentifier.Equals(fixIdentifier, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get verbal description of the clearance
    /// </summary>
    public string GetVerbalDescription()
    {
        if (!_legs.Any())
            return "own navigation";

        var destination = _legs.Last().TargetIdentifier;
        return _legs.Count == 1 ? $"direct to {destination}" : $"via route to {destination}";
    }

    /// <summary>
    /// Advance to next leg (called when aircraft reaches current waypoint)
    /// </summary>
    public ClearanceLeg? AdvanceToNextLeg()
    {
        if (_legs.Any())
        {
            _legs.RemoveAt(0);
        }
        return CurrentLeg;
    }

    /// <summary>
    /// Insert a new leg at the beginning (for immediate navigation)
    /// </summary>
    public void InsertImmediateLeg(ClearanceLeg leg)
    {
        _legs.Insert(0, leg);
    }

    /// <summary>
    /// Clear all legs and replace with direct-to clearance
    /// </summary>
    public void ClearAndDirectTo(string fixIdentifier)
    {
        _legs.Clear();
        _legs.Add(new ClearanceLeg 
        { 
            TargetIdentifier = fixIdentifier,
            NavigationMode = NavigationMode.DirectTo,
            AltitudeRestriction = ClearanceAltitudeRestriction.None,
            SpeedRestriction = ClearanceSpeedRestriction.None
        });
    }

    #region ICollection Implementation
    public int Count => _legs.Count;
    public bool IsReadOnly => false;

    public void Add(ClearanceLeg item) => _legs.Add(item);
    public void Clear() => _legs.Clear();
    public bool Contains(ClearanceLeg item) => _legs.Contains(item);
    public void CopyTo(ClearanceLeg[] array, int arrayIndex) => _legs.CopyTo(array, arrayIndex);
    public bool Remove(ClearanceLeg item) => _legs.Remove(item);
    public IEnumerator<ClearanceLeg> GetEnumerator() => _legs.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}

/// <summary>
/// Represents a single leg of a route clearance
/// </summary>
public class ClearanceLeg
{
    /// <summary>
    /// Target fix, runway, or waypoint identifier
    /// </summary>
    public string TargetIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Navigation mode for this leg
    /// </summary>
    public NavigationMode NavigationMode { get; set; } = NavigationMode.DirectTo;

    /// <summary>
    /// Assigned heading if using heading mode
    /// </summary>
    public float? AssignedHeadingDegrees { get; set; }

    /// <summary>
    /// Altitude restriction for this leg
    /// </summary>
    public ClearanceAltitudeRestriction AltitudeRestriction { get; set; } = ClearanceAltitudeRestriction.None;

    /// <summary>
    /// Target altitude if restriction applies
    /// </summary>
    public float? TargetAltitudeFt { get; set; }

    /// <summary>
    /// Speed restriction for this leg
    /// </summary>
    public ClearanceSpeedRestriction SpeedRestriction { get; set; } = ClearanceSpeedRestriction.None;

    /// <summary>
    /// Target speed if restriction applies
    /// </summary>
    public float? TargetSpeedKnots { get; set; }

    /// <summary>
    /// Expected overfly time (for time-based clearances)
    /// </summary>
    public DateTime? ExpectedTime { get; set; }

    /// <summary>
    /// Create direct-to clearance leg
    /// </summary>
    public static ClearanceLeg DirectTo(string fixIdentifier)
    {
        return new ClearanceLeg
        {
            TargetIdentifier = fixIdentifier,
            NavigationMode = NavigationMode.DirectTo
        };
    }

    /// <summary>
    /// Create heading clearance leg
    /// </summary>
    public static ClearanceLeg FlyHeading(float headingDegrees, string? limitIdentifier = null)
    {
        return new ClearanceLeg
        {
            TargetIdentifier = limitIdentifier ?? "radar vectors",
            NavigationMode = NavigationMode.Heading,
            AssignedHeadingDegrees = headingDegrees
        };
    }

    /// <summary>
    /// Create approach clearance leg
    /// </summary>
    public static ClearanceLeg ApproachClearance(string runwayIdentifier, string approachType = "ILS")
    {
        return new ClearanceLeg
        {
            TargetIdentifier = runwayIdentifier,
            NavigationMode = NavigationMode.Approach
        };
    }

    public override string ToString()
    {
        return NavigationMode switch
        {
            NavigationMode.DirectTo => $"Direct to {TargetIdentifier}",
            NavigationMode.Heading => $"Heading {AssignedHeadingDegrees:F0}°",
            NavigationMode.Approach => $"{TargetIdentifier} approach",
            NavigationMode.Hold => $"Hold at {TargetIdentifier}",
            _ => TargetIdentifier
        };
    }
}

/// <summary>
/// Navigation modes for clearance legs
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// Direct navigation to waypoint
    /// </summary>
    DirectTo,

    /// <summary>
    /// Fly assigned heading
    /// </summary>
    Heading,

    /// <summary>
    /// Follow published route/procedure
    /// </summary>
    Procedure,

    /// <summary>
    /// Approach clearance
    /// </summary>
    Approach,

    /// <summary>
    /// Hold at waypoint
    /// </summary>
    Hold,

    /// <summary>
    /// Intercept specific track/course
    /// </summary>
    Intercept
}

/// <summary>
/// Altitude restriction types for clearance legs
/// </summary>
public enum ClearanceAltitudeRestriction
{
    None,
    MaintainAtOrAbove,
    MaintainAtOrBelow,
    MaintainExactly,
    CrossAtOrAbove,
    CrossAtOrBelow,
    CrossExactly
}

/// <summary>
/// Speed restriction types for clearance legs
/// </summary>
public enum ClearanceSpeedRestriction
{
    None,
    MaintainAtOrBelow,
    MaintainExactly,
    MaintainAtOrAbove
}