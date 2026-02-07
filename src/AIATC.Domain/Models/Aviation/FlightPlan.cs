using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Flight plan with routing and performance data
/// </summary>
public class FlightPlan
{
    /// <summary>
    /// Flight plan ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Aircraft callsign
    /// </summary>
    public string Callsign { get; set; } = string.Empty;

    /// <summary>
    /// Aircraft type
    /// </summary>
    public string AircraftType { get; set; } = string.Empty;

    /// <summary>
    /// Departure airport ICAO code
    /// </summary>
    public string DepartureAirport { get; set; } = string.Empty;

    /// <summary>
    /// Destination airport ICAO code
    /// </summary>
    public string DestinationAirport { get; set; } = string.Empty;

    /// <summary>
    /// Alternate airport ICAO code
    /// </summary>
    public string? AlternateAirport { get; set; }

    /// <summary>
    /// Planned cruise altitude in feet
    /// </summary>
    public int CruiseAltitudeFt { get; set; }

    /// <summary>
    /// Planned cruise speed in knots
    /// </summary>
    public int CruiseSpeedKnots { get; set; }

    /// <summary>
    /// Route waypoints in sequence
    /// </summary>
    public List<RouteWaypoint> Route { get; set; } = new();

    /// <summary>
    /// Assigned SID (Standard Instrument Departure)
    /// </summary>
    public string? AssignedSid { get; set; }

    /// <summary>
    /// Assigned STAR (Standard Terminal Arrival Route)
    /// </summary>
    public string? AssignedStar { get; set; }

    /// <summary>
    /// Assigned approach
    /// </summary>
    public string? AssignedApproach { get; set; }

    /// <summary>
    /// Flight time estimate in minutes
    /// </summary>
    public int EstimatedFlightTimeMinutes { get; set; }

    /// <summary>
    /// Fuel endurance in minutes
    /// </summary>
    public int FuelEnduranceMinutes { get; set; }

    /// <summary>
    /// Current waypoint index in route
    /// </summary>
    public int CurrentWaypointIndex { get; set; } = 0;

    /// <summary>
    /// Get next waypoint in route
    /// </summary>
    public RouteWaypoint? GetNextWaypoint()
    {
        if (CurrentWaypointIndex >= Route.Count - 1)
            return null;
        
        return Route[CurrentWaypointIndex + 1];
    }

    /// <summary>
    /// Get current target waypoint
    /// </summary>
    public RouteWaypoint? GetCurrentWaypoint()
    {
        if (CurrentWaypointIndex >= Route.Count)
            return null;
            
        return Route[CurrentWaypointIndex];
    }

    /// <summary>
    /// Advance to next waypoint
    /// </summary>
    public void AdvanceToNextWaypoint()
    {
        if (CurrentWaypointIndex < Route.Count - 1)
            CurrentWaypointIndex++;
    }
}

/// <summary>
/// Waypoint in a flight plan route
/// </summary>
public class RouteWaypoint
{
    /// <summary>
    /// Waypoint identifier
    /// </summary>
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Position in nautical miles
    /// </summary>
    public Vector2 PositionNm { get; set; }

    /// <summary>
    /// Altitude restriction at this waypoint
    /// </summary>
    public AltitudeRestriction? AltitudeRestriction { get; set; }

    /// <summary>
    /// Speed restriction at this waypoint
    /// </summary>
    public SpeedRestriction? SpeedRestriction { get; set; }

    /// <summary>
    /// Estimated time of arrival
    /// </summary>
    public DateTime? EstimatedTimeOfArrival { get; set; }
}

/// <summary>
/// Altitude restriction types
/// </summary>
public class AltitudeRestriction
{
    public AltitudeConstraintType Type { get; set; }
    public int AltitudeFt { get; set; }
    public int? AltitudeFt2 { get; set; } // For between restrictions
}

/// <summary>
/// Speed restriction types
/// </summary>
public class SpeedRestriction
{
    public SpeedConstraintType Type { get; set; }
    public int SpeedKnots { get; set; }
}

public enum AltitudeConstraintType
{
    At,           // =
    AtOrAbove,    // +
    AtOrBelow,    // -
    Between       // Between two altitudes
}

public enum SpeedConstraintType
{
    At,           // =
    AtOrAbove,    // +
    AtOrBelow     // -
}