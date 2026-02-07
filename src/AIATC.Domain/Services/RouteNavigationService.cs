using AIATC.Domain.Models;
using AIATC.Domain.Models.Aviation;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for managing aircraft route navigation and clearance execution
/// </summary>
public interface IRouteNavigationService
{
    /// <summary>
    /// Execute current clearance leg for aircraft
    /// </summary>
    void ExecuteCurrentClearance(AircraftModel aircraft, IEnumerable<Fix> availableFixes, float deltaTimeSeconds);

    /// <summary>
    /// Check if aircraft has reached current waypoint
    /// </summary>
    bool HasReachedWaypoint(AircraftModel aircraft, string waypointIdentifier, IEnumerable<Fix> availableFixes);

    /// <summary>
    /// Calculate navigation targets for current clearance
    /// </summary>
    NavigationTargets CalculateNavigationTargets(AircraftModel aircraft, RouteClearance clearance, IEnumerable<Fix> availableFixes);

    /// <summary>
    /// Validate clearance against available navigation aids
    /// </summary>
    ClearanceValidationResult ValidateClearance(RouteClearance clearance, IEnumerable<Fix> availableFixes);

    /// <summary>
    /// Create optimized route between two points
    /// </summary>
    RouteClearance CreateOptimizedRoute(Vector2 startPosition, Vector2 endPosition, IEnumerable<Fix> availableFixes);
}

/// <summary>
/// Implementation of route navigation service
/// </summary>
public class RouteNavigationService : IRouteNavigationService
{
    private const float WaypointToleranceNm = 0.5f; // Distance tolerance for waypoint passage
    private const float MaxTurnRateDegPerSec = 3.0f; // Maximum turn rate in degrees per second
    
    public void ExecuteCurrentClearance(AircraftModel aircraft, IEnumerable<Fix> availableFixes, float deltaTimeSeconds)
    {
        if (aircraft.RouteClearance?.CurrentLeg == null)
            return;

        var currentLeg = aircraft.RouteClearance.CurrentLeg;
        var navigationTargets = CalculateNavigationTargets(aircraft, aircraft.RouteClearance, availableFixes);

        // Apply navigation targets to aircraft
        ApplyNavigationTargets(aircraft, navigationTargets, deltaTimeSeconds);

        // Check for waypoint passage
        if (HasReachedWaypoint(aircraft, currentLeg.TargetIdentifier, availableFixes))
        {
            OnWaypointPassed(aircraft, currentLeg);
        }
    }

    public bool HasReachedWaypoint(AircraftModel aircraft, string waypointIdentifier, IEnumerable<Fix> availableFixes)
    {
        var waypoint = availableFixes.FirstOrDefault(f => 
            f.Identifier.Equals(waypointIdentifier, StringComparison.OrdinalIgnoreCase));
        
        if (waypoint == null)
            return false;

        var distanceToWaypoint = Vector2.Distance(aircraft.PositionNm, waypoint.PositionNm);
        return distanceToWaypoint <= WaypointToleranceNm;
    }

    public NavigationTargets CalculateNavigationTargets(AircraftModel aircraft, RouteClearance clearance, IEnumerable<Fix> availableFixes)
    {
        var targets = new NavigationTargets();
        var currentLeg = clearance.CurrentLeg;
        
        if (currentLeg == null)
            return targets;

        switch (currentLeg.NavigationMode)
        {
            case NavigationMode.DirectTo:
                CalculateDirectToTargets(aircraft, currentLeg, availableFixes, targets);
                break;

            case NavigationMode.Heading:
                CalculateHeadingTargets(aircraft, currentLeg, targets);
                break;

            case NavigationMode.Approach:
                CalculateApproachTargets(aircraft, currentLeg, availableFixes, targets);
                break;

            case NavigationMode.Hold:
                CalculateHoldTargets(aircraft, currentLeg, availableFixes, targets);
                break;
        }

        // Apply altitude and speed restrictions
        ApplyRestrictions(currentLeg, targets);

        return targets;
    }

    public ClearanceValidationResult ValidateClearance(RouteClearance clearance, IEnumerable<Fix> availableFixes)
    {
        var result = new ClearanceValidationResult { IsValid = true };
        
        foreach (var leg in clearance.RemainingLegs)
        {
            if (leg.NavigationMode == NavigationMode.DirectTo || leg.NavigationMode == NavigationMode.Hold)
            {
                var fix = availableFixes.FirstOrDefault(f => 
                    f.Identifier.Equals(leg.TargetIdentifier, StringComparison.OrdinalIgnoreCase));
                
                if (fix == null)
                {
                    result.IsValid = false;
                    result.ValidationErrors.Add($"Fix '{leg.TargetIdentifier}' not found");
                }
            }

            // Validate altitude restrictions
            if (leg.AltitudeRestriction != ClearanceAltitudeRestriction.None && !leg.TargetAltitudeFt.HasValue)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Altitude restriction specified without target altitude for {leg.TargetIdentifier}");
            }

            // Validate speed restrictions
            if (leg.SpeedRestriction != ClearanceSpeedRestriction.None && !leg.TargetSpeedKnots.HasValue)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Speed restriction specified without target speed for {leg.TargetIdentifier}");
            }

            // Validate heading assignments
            if (leg.NavigationMode == NavigationMode.Heading && !leg.AssignedHeadingDegrees.HasValue)
            {
                result.IsValid = false;
                result.ValidationErrors.Add($"Heading navigation mode specified without assigned heading");
            }
        }

        return result;
    }

    public RouteClearance CreateOptimizedRoute(Vector2 startPosition, Vector2 endPosition, IEnumerable<Fix> availableFixes)
    {
        var clearance = new RouteClearance();
        
        // Simple implementation: find nearest waypoint to destination
        var nearestFix = availableFixes
            .OrderBy(f => Vector2.Distance(f.PositionNm, endPosition))
            .FirstOrDefault();

        if (nearestFix != null && Vector2.Distance(nearestFix.PositionNm, endPosition) < 10.0f)
        {
            clearance.Add(ClearanceLeg.DirectTo(nearestFix.Identifier));
        }

        return clearance;
    }

    private void CalculateDirectToTargets(AircraftModel aircraft, ClearanceLeg leg, IEnumerable<Fix> availableFixes, NavigationTargets targets)
    {
        var targetFix = availableFixes.FirstOrDefault(f => 
            f.Identifier.Equals(leg.TargetIdentifier, StringComparison.OrdinalIgnoreCase));
        
        if (targetFix == null)
            return;

        // Calculate bearing to target
        var vectorToTarget = targetFix.PositionNm - aircraft.PositionNm;
        var bearingToTarget = (float)(Math.Atan2(vectorToTarget.X, vectorToTarget.Y) * 180.0 / Math.PI);
        
        // Normalize bearing to 0-360 range
        if (bearingToTarget < 0) bearingToTarget += 360;

        targets.TargetHeading = bearingToTarget;
        targets.NavigationType = NavigationType.DirectTo;
        targets.TargetPosition = targetFix.PositionNm;
    }

    private void CalculateHeadingTargets(AircraftModel aircraft, ClearanceLeg leg, NavigationTargets targets)
    {
        if (leg.AssignedHeadingDegrees.HasValue)
        {
            targets.TargetHeading = leg.AssignedHeadingDegrees.Value;
            targets.NavigationType = NavigationType.Heading;
        }
    }

    private void CalculateApproachTargets(AircraftModel aircraft, ClearanceLeg leg, IEnumerable<Fix> availableFixes, NavigationTargets targets)
    {
        // Simplified approach navigation - would normally involve complex procedure following
        var approachFix = availableFixes.FirstOrDefault(f => 
            f.Identifier.Contains("ILS") || f.Identifier.Contains(leg.TargetIdentifier));
        
        if (approachFix != null)
        {
            CalculateDirectToTargets(aircraft, leg, new[] { approachFix }, targets);
            targets.NavigationType = NavigationType.Approach;
        }
    }

    private void CalculateHoldTargets(AircraftModel aircraft, ClearanceLeg leg, IEnumerable<Fix> availableFixes, NavigationTargets targets)
    {
        var holdFix = availableFixes.FirstOrDefault(f => 
            f.Identifier.Equals(leg.TargetIdentifier, StringComparison.OrdinalIgnoreCase));
        
        if (holdFix != null)
        {
            // Simplified hold pattern - would normally involve racetrack pattern calculation
            CalculateDirectToTargets(aircraft, leg, new[] { holdFix }, targets);
            targets.NavigationType = NavigationType.Hold;
        }
    }

    private void ApplyRestrictions(ClearanceLeg leg, NavigationTargets targets)
    {
        // Apply altitude restrictions
        if (leg.AltitudeRestriction != ClearanceAltitudeRestriction.None && leg.TargetAltitudeFt.HasValue)
        {
            targets.TargetAltitude = leg.TargetAltitudeFt.Value;
        }

        // Apply speed restrictions
        if (leg.SpeedRestriction != ClearanceSpeedRestriction.None && leg.TargetSpeedKnots.HasValue)
        {
            targets.TargetSpeed = leg.TargetSpeedKnots.Value;
        }
    }

    private void ApplyNavigationTargets(AircraftModel aircraft, NavigationTargets targets, float deltaTimeSeconds)
    {
        // Apply heading target with turn rate limiting
        if (targets.TargetHeading.HasValue)
        {
            var headingError = targets.TargetHeading.Value - aircraft.HeadingDegrees;
            
            // Normalize heading error to -180 to +180 range
            while (headingError > 180) headingError -= 360;
            while (headingError < -180) headingError += 360;

            // Limit turn rate
            var maxTurnThisFrame = MaxTurnRateDegPerSec * deltaTimeSeconds;
            var turnAmount = Math.Sign(headingError) * Math.Min(Math.Abs(headingError), maxTurnThisFrame);
            
            aircraft.TargetHeadingDegrees = aircraft.HeadingDegrees + turnAmount;
        }

        // Apply altitude target
        if (targets.TargetAltitude.HasValue)
        {
            aircraft.TargetAltitudeFt = targets.TargetAltitude.Value;
        }

        // Apply speed target
        if (targets.TargetSpeed.HasValue)
        {
            aircraft.TargetSpeedKnots = targets.TargetSpeed.Value;
        }
    }

    private void OnWaypointPassed(AircraftModel aircraft, ClearanceLeg passedLeg)
    {
        // Advance to next leg
        aircraft.RouteClearance?.AdvanceToNextLeg();
        
        // Add to position history for tracking
        // This would be handled by the simulation service
    }
}

/// <summary>
/// Navigation targets calculated for current clearance
/// </summary>
public class NavigationTargets
{
    public float? TargetHeading { get; set; }
    public float? TargetAltitude { get; set; }
    public float? TargetSpeed { get; set; }
    public Vector2? TargetPosition { get; set; }
    public NavigationType NavigationType { get; set; } = NavigationType.None;
}

/// <summary>
/// Navigation types for different clearance modes
/// </summary>
public enum NavigationType
{
    None,
    DirectTo,
    Heading,
    Approach,
    Hold,
    Intercept
}

/// <summary>
/// Result of clearance validation
/// </summary>
public class ClearanceValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
}