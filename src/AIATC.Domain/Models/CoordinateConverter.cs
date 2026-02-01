using System;

namespace AIATC.Domain.Models;

/// <summary>
/// Utility for converting between latitude/longitude and local ENU (East-North-Up) coordinates.
/// ENU coordinates are in meters, positions in simulation are in nautical miles.
/// </summary>
public static class CoordinateConverter
{
    private const double MetersToNauticalMiles = 1.0 / 1852.0;
    private const double NauticalMilesToMeters = 1852.0;

    /// <summary>
    /// Converts lat/lon to local ENU (East-North-Up) coordinates in meters.
    /// </summary>
    /// <param name="lat">Latitude in degrees</param>
    /// <param name="lon">Longitude in degrees</param>
    /// <param name="lat0">Reference latitude in degrees</param>
    /// <param name="lon0">Reference longitude in degrees</param>
    /// <returns>Tuple of (east_m, north_m)</returns>
    public static (double eastM, double northM) LatLonToEnu(double lat, double lon, double lat0, double lon0)
    {
        var dlat = DegreesToRadians(lat - lat0);
        var dlon = DegreesToRadians(lon - lon0);

        var eastM = SimulationConstants.EarthRadiusMeters * dlon * Math.Cos(DegreesToRadians(lat0));
        var northM = SimulationConstants.EarthRadiusMeters * dlat;

        return (eastM, northM);
    }

    /// <summary>
    /// Converts local ENU coordinates in meters to lat/lon.
    /// </summary>
    /// <param name="eastM">East coordinate in meters</param>
    /// <param name="northM">North coordinate in meters</param>
    /// <param name="lat0">Reference latitude in degrees</param>
    /// <param name="lon0">Reference longitude in degrees</param>
    /// <returns>Tuple of (lat, lon) in degrees</returns>
    public static (double lat, double lon) EnuToLatLon(double eastM, double northM, double lat0, double lon0)
    {
        var dlat = northM / SimulationConstants.EarthRadiusMeters;
        var dlon = eastM / (SimulationConstants.EarthRadiusMeters * Math.Cos(DegreesToRadians(lat0)));

        var lat = lat0 + RadiansToDegrees(dlat);
        var lon = lon0 + RadiansToDegrees(dlon);

        return (lat, lon);
    }

    /// <summary>
    /// Converts lat/lon to simulation Vector2 in nautical miles.
    /// </summary>
    public static Vector2 LatLonToVector2Nm(double lat, double lon, double lat0, double lon0)
    {
        var (eastM, northM) = LatLonToEnu(lat, lon, lat0, lon0);
        return new Vector2(
            (float)(eastM * MetersToNauticalMiles),
            (float)(northM * MetersToNauticalMiles)
        );
    }

    /// <summary>
    /// Converts simulation Vector2 in nautical miles to lat/lon.
    /// </summary>
    public static (double lat, double lon) Vector2NmToLatLon(Vector2 positionNm, double lat0, double lon0)
    {
        var eastM = positionNm.X * NauticalMilesToMeters;
        var northM = positionNm.Y * NauticalMilesToMeters;
        return EnuToLatLon(eastM, northM, lat0, lon0);
    }

    /// <summary>
    /// Calculates great circle distance between two lat/lon points in nautical miles.
    /// </summary>
    public static double GreatCircleDistanceNm(double lat1, double lon1, double lat2, double lon2)
    {
        var lat1Rad = DegreesToRadians(lat1);
        var lat2Rad = DegreesToRadians(lat2);
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLon = DegreesToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceM = SimulationConstants.EarthRadiusMeters * c;

        return distanceM * MetersToNauticalMiles;
    }

    /// <summary>
    /// Calculates initial bearing from point 1 to point 2 in degrees.
    /// </summary>
    public static double InitialBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var lat1Rad = DegreesToRadians(lat1);
        var lat2Rad = DegreesToRadians(lat2);
        var dLon = DegreesToRadians(lon2 - lon1);

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearingRad = Math.Atan2(y, x);
        return (RadiansToDegrees(bearingRad) + 360) % 360;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
    private static double RadiansToDegrees(double radians) => radians * 180.0 / Math.PI;
}
