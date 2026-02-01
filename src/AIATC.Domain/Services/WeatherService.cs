using AIATC.Domain.Models.Weather;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for managing weather conditions
/// </summary>
public class WeatherService
{
    private readonly Dictionary<string, WeatherConditions> _weatherByLocation = new();
    private readonly Random _random = new();

    /// <summary>
    /// Event raised when weather conditions change
    /// </summary>
    public event EventHandler<WeatherChangedEventArgs>? WeatherChanged;

    /// <summary>
    /// Gets weather for a location
    /// </summary>
    public WeatherConditions GetWeather(string locationId)
    {
        if (_weatherByLocation.TryGetValue(locationId, out var weather))
            return weather;

        // Create default clear weather if not found
        var defaultWeather = WeatherConditions.CreateClear(locationId);
        _weatherByLocation[locationId] = defaultWeather;
        return defaultWeather;
    }

    /// <summary>
    /// Sets weather for a location
    /// </summary>
    public void SetWeather(string locationId, WeatherConditions weather)
    {
        weather.LocationId = locationId;
        weather.ObservationTime = DateTime.UtcNow;

        var previousWeather = _weatherByLocation.TryGetValue(locationId, out var prev) ? prev : null;
        _weatherByLocation[locationId] = weather;

        WeatherChanged?.Invoke(this, new WeatherChangedEventArgs
        {
            LocationId = locationId,
            NewWeather = weather,
            PreviousWeather = previousWeather
        });
    }

    /// <summary>
    /// Updates weather to simulate gradual changes
    /// </summary>
    public void UpdateWeather(string locationId, float deltaTimeSeconds)
    {
        if (!_weatherByLocation.TryGetValue(locationId, out var weather))
            return;

        // Small random changes to wind
        if (weather.WindLayers.Count > 0)
        {
            foreach (var wind in weather.WindLayers)
            {
                // Wind direction shifts ±5 degrees per update (slowly)
                if (_random.NextDouble() < 0.1)
                {
                    wind.DirectionDegrees += (float)(_random.NextDouble() - 0.5) * 10f;
                    wind.DirectionDegrees = (wind.DirectionDegrees + 360) % 360;
                }

                // Wind speed varies ±2 knots
                if (_random.NextDouble() < 0.1)
                {
                    wind.SpeedKnots += (float)(_random.NextDouble() - 0.5) * 4f;
                    wind.SpeedKnots = Math.Max(0, wind.SpeedKnots);
                }
            }
        }

        // Update observation time
        weather.ObservationTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Generates random weather based on difficulty
    /// </summary>
    public WeatherConditions GenerateRandomWeather(string locationId, WeatherDifficulty difficulty)
    {
        return difficulty switch
        {
            WeatherDifficulty.Easy => GenerateEasyWeather(locationId),
            WeatherDifficulty.Medium => GenerateMediumWeather(locationId),
            WeatherDifficulty.Hard => GenerateHardWeather(locationId),
            WeatherDifficulty.Extreme => GenerateExtremeWeather(locationId),
            _ => WeatherConditions.CreateClear(locationId)
        };
    }

    private WeatherConditions GenerateEasyWeather(string locationId)
    {
        // Light winds, clear skies
        var windDir = _random.Next(0, 360);
        var windSpeed = _random.Next(3, 10);

        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(windDir, windSpeed)
            },
            CloudLayers = new List<CloudLayer>
            {
                CloudLayer.CreateScattered(4500)
            },
            Visibility = VisibilityConditions.CreateClear(),
            AltimeterInHg = 29.92f + (float)(_random.NextDouble() - 0.5) * 0.3f,
            TemperatureCelsius = 15f + (float)(_random.NextDouble() - 0.5) * 10f,
            DewpointCelsius = 10f
        };
    }

    private WeatherConditions GenerateMediumWeather(string locationId)
    {
        // Moderate winds, some clouds
        var windDir = _random.Next(0, 360);
        var windSpeed = _random.Next(10, 20);
        var gustSpeed = windSpeed + _random.Next(5, 12);

        var clouds = new List<CloudLayer>
        {
            CloudLayer.CreateScattered(2500),
            CloudLayer.CreateCeiling(5000, CloudCoverage.Broken)
        };

        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(windDir, windSpeed, gustSpeed)
            },
            CloudLayers = clouds,
            Visibility = new VisibilityConditions
            {
                VisibilityMiles = 6 + (float)_random.NextDouble() * 4,
                Precipitation = PrecipitationType.None,
                Obscuration = ObscurationType.Haze
            },
            AltimeterInHg = 29.92f + (float)(_random.NextDouble() - 0.5) * 0.4f,
            TemperatureCelsius = 12f + (float)(_random.NextDouble() - 0.5) * 15f,
            DewpointCelsius = 8f
        };
    }

    private WeatherConditions GenerateHardWeather(string locationId)
    {
        // Strong winds, low ceiling
        var windDir = _random.Next(0, 360);
        var windSpeed = _random.Next(18, 30);
        var gustSpeed = windSpeed + _random.Next(8, 15);

        var clouds = new List<CloudLayer>
        {
            CloudLayer.CreateCeiling(1200, CloudCoverage.Overcast)
        };

        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(windDir, windSpeed, gustSpeed),
                WindLayer.CreateAloft(windDir + 20, windSpeed + 10, 3000, 10000)
            },
            CloudLayers = clouds,
            Visibility = new VisibilityConditions
            {
                VisibilityMiles = 3 + (float)_random.NextDouble() * 2,
                Precipitation = PrecipitationType.Rain,
                Intensity = PrecipitationIntensity.Light,
                Obscuration = ObscurationType.Mist
            },
            AltimeterInHg = 29.92f + (float)(_random.NextDouble() - 0.5) * 0.5f,
            TemperatureCelsius = 8f + (float)(_random.NextDouble() - 0.5) * 10f,
            DewpointCelsius = 7f
        };
    }

    private WeatherConditions GenerateExtremeWeather(string locationId)
    {
        // Very strong winds, very low ceiling, poor visibility
        var windDir = _random.Next(0, 360);
        var windSpeed = _random.Next(25, 40);
        var gustSpeed = windSpeed + _random.Next(10, 20);

        var clouds = new List<CloudLayer>
        {
            CloudLayer.CreateCeiling(400, CloudCoverage.Overcast)
        };

        return new WeatherConditions
        {
            LocationId = locationId,
            ObservationTime = DateTime.UtcNow,
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(windDir, windSpeed, gustSpeed),
                WindLayer.CreateAloft(windDir + 40, windSpeed + 20, 3000, 10000),
                WindLayer.CreateAloft(windDir + 60, windSpeed + 40, 10000, 30000)
            },
            CloudLayers = clouds,
            Visibility = new VisibilityConditions
            {
                VisibilityMiles = 0.5f + (float)_random.NextDouble() * 1.5f,
                Precipitation = PrecipitationType.Rain,
                Intensity = PrecipitationIntensity.Heavy,
                Obscuration = ObscurationType.Fog
            },
            AltimeterInHg = 29.92f + (float)(_random.NextDouble() - 0.5) * 0.7f,
            TemperatureCelsius = 5f + (float)(_random.NextDouble() - 0.5) * 8f,
            DewpointCelsius = 4f
        };
    }

    /// <summary>
    /// Gets all locations with weather data
    /// </summary>
    public IEnumerable<string> GetLocations()
    {
        return _weatherByLocation.Keys;
    }

    /// <summary>
    /// Clears all weather data
    /// </summary>
    public void Clear()
    {
        _weatherByLocation.Clear();
    }
}

/// <summary>
/// Weather difficulty levels
/// </summary>
public enum WeatherDifficulty
{
    Easy,
    Medium,
    Hard,
    Extreme
}

/// <summary>
/// Event args for weather changes
/// </summary>
public class WeatherChangedEventArgs : EventArgs
{
    public string LocationId { get; set; } = string.Empty;
    public WeatherConditions? NewWeather { get; set; }
    public WeatherConditions? PreviousWeather { get; set; }
}
