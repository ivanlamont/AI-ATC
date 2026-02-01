using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIATC.Domain.Models.Weather;

/// <summary>
/// Complete weather conditions for a location
/// </summary>
public class WeatherConditions
{
    /// <summary>
    /// Airport or location identifier
    /// </summary>
    public string LocationId { get; set; } = string.Empty;

    /// <summary>
    /// Observation time (UTC)
    /// </summary>
    public DateTime ObservationTime { get; set; }

    /// <summary>
    /// Wind layers (surface to altitude)
    /// </summary>
    public List<WindLayer> WindLayers { get; set; } = new();

    /// <summary>
    /// Cloud layers (lowest to highest)
    /// </summary>
    public List<CloudLayer> CloudLayers { get; set; } = new();

    /// <summary>
    /// Visibility conditions
    /// </summary>
    public VisibilityConditions Visibility { get; set; } = VisibilityConditions.CreateClear();

    /// <summary>
    /// Altimeter setting in inches of mercury
    /// </summary>
    public float AltimeterInHg { get; set; } = 29.92f;

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public float TemperatureCelsius { get; set; } = 15f;

    /// <summary>
    /// Dewpoint in Celsius
    /// </summary>
    public float DewpointCelsius { get; set; } = 10f;

    /// <summary>
    /// Field elevation in feet MSL
    /// </summary>
    public float FieldElevationFt { get; set; } = 0f;

    /// <summary>
    /// Gets the wind at a specific altitude
    /// </summary>
    public WindLayer? GetWindAtAltitude(float altitudeFt)
    {
        // Find the layer that contains this altitude
        var layer = WindLayers.FirstOrDefault(w => w.ContainsAltitude(altitudeFt));

        // If no layer found, use the closest one
        if (layer == null && WindLayers.Count > 0)
        {
            layer = WindLayers
                .OrderBy(w => Math.Abs(w.BaseAltitudeFt - altitudeFt))
                .First();
        }

        return layer;
    }

    /// <summary>
    /// Gets the ceiling altitude in feet AGL (null if no ceiling)
    /// </summary>
    public float? GetCeilingAgl()
    {
        var ceilingLayer = CloudLayers
            .Where(c => c.IsCeiling())
            .OrderBy(c => c.BaseAltitudeAgl)
            .FirstOrDefault();

        return ceilingLayer?.BaseAltitudeAgl;
    }

    /// <summary>
    /// Gets the wind effect on ground speed for an aircraft
    /// </summary>
    public Vector2 GetWindEffect(float altitudeFt)
    {
        var wind = GetWindAtAltitude(altitudeFt);
        return wind?.GetWindVector() ?? new Vector2(0, 0);
    }

    /// <summary>
    /// Checks if the weather is suitable for VFR flight
    /// </summary>
    public bool IsVfr()
    {
        var ceiling = GetCeilingAgl();

        // VFR: Ceiling >= 3000 ft AGL and visibility >= 5 SM
        return Visibility.VisibilityMiles >= 5.0f &&
               (!ceiling.HasValue || ceiling.Value >= 3000);
    }

    /// <summary>
    /// Gets flight category (VFR, MVFR, IFR, LIFR)
    /// </summary>
    public string GetFlightCategory()
    {
        return Visibility.GetFlightCategory(GetCeilingAgl());
    }

    /// <summary>
    /// Calculates density altitude in feet
    /// </summary>
    public float GetDensityAltitude()
    {
        // Simplified density altitude calculation
        var pressureAlt = FieldElevationFt + (29.92f - AltimeterInHg) * 1000f;
        var isaTemp = 15f - (pressureAlt / 1000f * 2f);
        var tempDiff = TemperatureCelsius - isaTemp;

        return pressureAlt + (120f * tempDiff);
    }

    /// <summary>
    /// Generates a METAR string
    /// </summary>
    public string ToMetarString()
    {
        var sb = new StringBuilder();

        // Type and location
        sb.Append($"METAR {LocationId} ");

        // Time
        sb.Append($"{ObservationTime:ddHHmm}Z ");

        // Wind
        if (WindLayers.Count > 0)
        {
            var surfaceWind = WindLayers.OrderBy(w => w.BaseAltitudeFt).First();
            sb.Append($"{surfaceWind.ToMetarString()} ");
        }

        // Visibility and weather
        sb.Append($"{Visibility.ToMetarString()} ");

        // Clouds
        if (CloudLayers.Count == 0)
        {
            sb.Append("CLR ");
        }
        else
        {
            foreach (var cloud in CloudLayers.OrderBy(c => c.BaseAltitudeAgl))
            {
                sb.Append($"{cloud.ToMetarString()} ");
            }
        }

        // Temperature/Dewpoint
        sb.Append($"{(int)TemperatureCelsius:D2}/{(int)DewpointCelsius:D2} ");

        // Altimeter
        var altimeter = ((int)(AltimeterInHg * 100)).ToString("D4");
        sb.Append($"A{altimeter}");

        return sb.ToString().Trim();
    }

    /// <summary>
    /// Creates clear, calm weather conditions
    /// </summary>
    public static WeatherConditions CreateClear(string locationId = "KJFK")
    {
        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(0, 0)
            },
            CloudLayers = new List<CloudLayer>
            {
                new CloudLayer { Coverage = CloudCoverage.Clear }
            },
            Visibility = VisibilityConditions.CreateClear(),
            AltimeterInHg = 29.92f,
            TemperatureCelsius = 15f,
            DewpointCelsius = 10f
        };
    }

    /// <summary>
    /// Creates typical IFR conditions
    /// </summary>
    public static WeatherConditions CreateIfr(string locationId = "KJFK")
    {
        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(180, 15, 22)
            },
            CloudLayers = new List<CloudLayer>
            {
                CloudLayer.CreateCeiling(800, CloudCoverage.Overcast)
            },
            Visibility = VisibilityConditions.CreateIfr(),
            AltimeterInHg = 29.85f,
            TemperatureCelsius = 10f,
            DewpointCelsius = 9f
        };
    }

    /// <summary>
    /// Creates windy conditions
    /// </summary>
    public static WeatherConditions CreateWindy(string locationId = "KJFK",
        float windDirection = 270, float windSpeed = 25)
    {
        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(windDirection, windSpeed, windSpeed + 10),
                WindLayer.CreateAloft(windDirection + 30, windSpeed + 15, 3000, 10000)
            },
            CloudLayers = new List<CloudLayer>
            {
                CloudLayer.CreateScattered(4500)
            },
            Visibility = VisibilityConditions.CreateClear(),
            AltimeterInHg = 30.15f,
            TemperatureCelsius = 12f,
            DewpointCelsius = 5f
        };
    }
}
