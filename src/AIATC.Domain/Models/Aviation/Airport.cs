using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Models.Aviation;

/// <summary>
/// Represents an airport with complete aviation data including runways and approaches
/// </summary>
public class Airport
{
    /// <summary>
    /// Unique identifier for Entity Framework
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ICAO airport identifier (e.g., KSFO, KLAX)
    /// </summary>
    public string IcaoCode { get; set; } = string.Empty;

    /// <summary>
    /// IATA airport code (e.g., SFO, LAX)
    /// </summary>
    public string? IataCode { get; set; }

    /// <summary>
    /// Airport name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Airport city
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Airport country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Airport reference position in nautical miles
    /// </summary>
    public Vector2 PositionNm { get; set; }

    /// <summary>
    /// Latitude in decimal degrees
    /// </summary>
    public double Latitude { get; set; }

    /// <summary>
    /// Longitude in decimal degrees
    /// </summary>
    public double Longitude { get; set; }

    /// <summary>
    /// Elevation in feet MSL (alias for ElevationFt)
    /// </summary>
    public float Elevation { get; set; }

    /// <summary>
    /// Airport elevation in feet MSL
    /// </summary>
    public float ElevationFt { get; set; }

    /// <summary>
    /// Magnetic variation at airport in degrees (+ East, - West)
    /// </summary>
    public float MagneticVariation { get; set; }

    /// <summary>
    /// Transition altitude in feet
    /// </summary>
    public int TransitionAltitudeFt { get; set; } = 18000;

    /// <summary>
    /// Longest runway in hundreds of feet
    /// </summary>
    public int LongestRunwayHundredsFt { get; set; }

    /// <summary>
    /// Speed limit altitude
    /// </summary>
    public int SpeedLimitAltitudeFt { get; set; } = 10000;

    /// <summary>
    /// Speed limit in knots below limit altitude
    /// </summary>
    public int SpeedLimitKnots { get; set; } = 250;

    /// <summary>
    /// Runways at this airport
    /// </summary>
    public List<Runway> Runways { get; set; } = new();

    /// <summary>
    /// Approaches available at this airport
    /// </summary>
    public List<Approach> Approaches { get; set; } = new();

    /// <summary>
    /// Fixes associated with this airport
    /// </summary>
    public List<Fix> Fixes { get; set; } = new();

    /// <summary>
    /// Communication frequencies
    /// </summary>
    public AirportCommunications Communications { get; set; } = new();
}