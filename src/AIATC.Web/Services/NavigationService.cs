using System.Numerics;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Web.Services;

/// <summary>
/// Navigation service for aviation calculations including great circle distance,
/// bearing calculations, and position interpolation for ATC operations
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Calculate great circle distance between two geographic points
    /// </summary>
    /// <param name="lat1">Starting latitude in degrees</param>
    /// <param name="lon1">Starting longitude in degrees</param>
    /// <param name="lat2">Ending latitude in degrees</param>
    /// <param name="lon2">Ending longitude in degrees</param>
    /// <returns>Distance in nautical miles</returns>
    double CalculateDistanceNm(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Calculate bearing from one point to another
    /// </summary>
    /// <param name="lat1">Starting latitude in degrees</param>
    /// <param name="lon1">Starting longitude in degrees</param>
    /// <param name="lat2">Ending latitude in degrees</param>
    /// <param name="lon2">Ending longitude in degrees</param>
    /// <returns>Bearing in degrees (0-360)</returns>
    double CalculateBearing(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Calculate position along great circle route at specified distance
    /// </summary>
    /// <param name="lat1">Starting latitude in degrees</param>
    /// <param name="lon1">Starting longitude in degrees</param>
    /// <param name="bearing">Initial bearing in degrees</param>
    /// <param name="distanceNm">Distance to travel in nautical miles</param>
    /// <returns>Destination coordinates</returns>
    (double lat, double lon) CalculateDestination(double lat1, double lon1, double bearing, double distanceNm);

    /// <summary>
    /// Convert geographic coordinates to radar screen coordinates
    /// </summary>
    /// <param name="lat">Latitude in degrees</param>
    /// <param name="lon">Longitude in degrees</param>
    /// <param name="centerLat">Radar center latitude in degrees</param>
    /// <param name="centerLon">Radar center longitude in degrees</param>
    /// <param name="rangeNm">Radar range in nautical miles</param>
    /// <param name="screenWidth">Screen width in pixels</param>
    /// <param name="screenHeight">Screen height in pixels</param>
    /// <param name="panOffset">Pan offset for radar view</param>
    /// <param name="zoomLevel">Zoom level multiplier</param>
    /// <returns>Screen coordinates</returns>
    Vector2 GeographicToScreen(double lat, double lon, double centerLat, double centerLon, 
        float rangeNm, int screenWidth, int screenHeight, Vector2 panOffset, float zoomLevel);

    /// <summary>
    /// Convert screen coordinates to geographic coordinates
    /// </summary>
    /// <param name="screenX">Screen X coordinate</param>
    /// <param name="screenY">Screen Y coordinate</param>
    /// <param name="centerLat">Radar center latitude in degrees</param>
    /// <param name="centerLon">Radar center longitude in degrees</param>
    /// <param name="rangeNm">Radar range in nautical miles</param>
    /// <param name="screenWidth">Screen width in pixels</param>
    /// <param name="screenHeight">Screen height in pixels</param>
    /// <param name="panOffset">Pan offset for radar view</param>
    /// <param name="zoomLevel">Zoom level multiplier</param>
    /// <returns>Geographic coordinates</returns>
    (double lat, double lon) ScreenToGeographic(float screenX, float screenY, double centerLat, double centerLon,
        float rangeNm, int screenWidth, int screenHeight, Vector2 panOffset, float zoomLevel);

    /// <summary>
    /// Calculate runway endpoints in geographic coordinates
    /// </summary>
    /// <param name="runway">Runway information</param>
    /// <returns>Start and end coordinates of runway</returns>
    (AIATC.Domain.Models.Vector2 start, AIATC.Domain.Models.Vector2 end) CalculateRunwayEndpoints(AIATC.Domain.Models.Aviation.Runway runway);

    /// <summary>
    /// Interpolate position between two geographic points
    /// </summary>
    /// <param name="lat1">Starting latitude</param>
    /// <param name="lon1">Starting longitude</param>
    /// <param name="lat2">Ending latitude</param>
    /// <param name="lon2">Ending longitude</param>
    /// <param name="fraction">Interpolation fraction (0.0 to 1.0)</param>
    /// <returns>Interpolated coordinates</returns>
    (double lat, double lon) InterpolatePosition(double lat1, double lon1, double lat2, double lon2, double fraction);
}

/// <summary>
/// Implementation of navigation service with professional aviation calculations
/// </summary>
public class NavigationService : INavigationService
{
    private const double EarthRadiusKm = 6371.0;
    private const double KmToNauticalMiles = 0.539957;
    private const double NauticalMilesToKm = 1.852;
    private const double DegreesToRadians = Math.PI / 180.0;
    private const double RadiansToDegrees = 180.0 / Math.PI;

    public double CalculateDistanceNm(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula for great circle distance
        var dLat = (lat2 - lat1) * DegreesToRadians;
        var dLon = (lon2 - lon1) * DegreesToRadians;
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * DegreesToRadians) * Math.Cos(lat2 * DegreesToRadians) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        var distanceKm = EarthRadiusKm * c;
        
        return distanceKm * KmToNauticalMiles;
    }

    public double CalculateBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var dLon = (lon2 - lon1) * DegreesToRadians;
        var lat1Rad = lat1 * DegreesToRadians;
        var lat2Rad = lat2 * DegreesToRadians;

        var y = Math.Sin(dLon) * Math.Cos(lat2Rad);
        var x = Math.Cos(lat1Rad) * Math.Sin(lat2Rad) -
                Math.Sin(lat1Rad) * Math.Cos(lat2Rad) * Math.Cos(dLon);

        var bearing = Math.Atan2(y, x) * RadiansToDegrees;
        return (bearing + 360) % 360; // Normalize to 0-360 degrees
    }

    public (double lat, double lon) CalculateDestination(double lat1, double lon1, double bearing, double distanceNm)
    {
        var distanceKm = distanceNm * NauticalMilesToKm;
        var angularDistance = distanceKm / EarthRadiusKm;
        var bearingRad = bearing * DegreesToRadians;
        var lat1Rad = lat1 * DegreesToRadians;
        var lon1Rad = lon1 * DegreesToRadians;

        var lat2Rad = Math.Asin(
            Math.Sin(lat1Rad) * Math.Cos(angularDistance) +
            Math.Cos(lat1Rad) * Math.Sin(angularDistance) * Math.Cos(bearingRad)
        );

        var lon2Rad = lon1Rad + Math.Atan2(
            Math.Sin(bearingRad) * Math.Sin(angularDistance) * Math.Cos(lat1Rad),
            Math.Cos(angularDistance) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad)
        );

        return (lat2Rad * RadiansToDegrees, lon2Rad * RadiansToDegrees);
    }

    public Vector2 GeographicToScreen(double lat, double lon, double centerLat, double centerLon,
        float rangeNm, int screenWidth, int screenHeight, Vector2 panOffset, float zoomLevel)
    {
        // Calculate distance and bearing from center
        var distance = CalculateDistanceNm(centerLat, centerLon, lat, lon);
        var bearing = CalculateBearing(centerLat, centerLon, lat, lon);

        // Convert to screen coordinates (bearing 0° = North = negative Y)
        var bearingRad = bearing * DegreesToRadians;
        var screenDistance = (distance / rangeNm) * (Math.Min(screenWidth, screenHeight) / 2.0) * zoomLevel;

        var x = screenWidth / 2.0f + (float)(screenDistance * Math.Sin(bearingRad)) + panOffset.X;
        var y = screenHeight / 2.0f - (float)(screenDistance * Math.Cos(bearingRad)) + panOffset.Y;

        return new Vector2(x, y);
    }

    public (double lat, double lon) ScreenToGeographic(float screenX, float screenY, double centerLat, double centerLon,
        float rangeNm, int screenWidth, int screenHeight, Vector2 panOffset, float zoomLevel)
    {
        // Convert screen position relative to center
        var deltaX = (screenX - panOffset.X) - screenWidth / 2.0f;
        var deltaY = screenHeight / 2.0f - (screenY - panOffset.Y);

        // Calculate distance and bearing
        var screenDistance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
        var bearing = Math.Atan2(deltaX, deltaY) * RadiansToDegrees;
        if (bearing < 0) bearing += 360;

        var actualDistance = (screenDistance / (Math.Min(screenWidth, screenHeight) / 2.0)) * rangeNm / zoomLevel;

        return CalculateDestination(centerLat, centerLon, bearing, actualDistance);
    }

    public (AIATC.Domain.Models.Vector2 start, AIATC.Domain.Models.Vector2 end) CalculateRunwayEndpoints(AIATC.Domain.Models.Aviation.Runway runway)
    {
        // Use the existing threshold and end positions from the runway model
        return (runway.ThresholdPositionNm, runway.EndPositionNm);
    }

    public (double lat, double lon) InterpolatePosition(double lat1, double lon1, double lat2, double lon2, double fraction)
    {
        // Spherical linear interpolation (slerp) for geographic coordinates
        var distance = CalculateDistanceNm(lat1, lon1, lat2, lon2);
        var bearing = CalculateBearing(lat1, lon1, lat2, lon2);
        var interpolatedDistance = distance * fraction;

        return CalculateDestination(lat1, lon1, bearing, interpolatedDistance);
    }
}