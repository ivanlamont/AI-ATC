using System.Linq;

namespace AIATC.Domain.Models.Weather;

/// <summary>
/// Extension methods for weather integration with existing systems
/// </summary>
public static class WeatherExtensions
{
    /// <summary>
    /// Converts a WindLayer to the legacy Wind model
    /// </summary>
    public static Wind ToWind(this WindLayer windLayer)
    {
        return new Wind(
            windLayer.DirectionDegrees,
            windLayer.SpeedKnots,
            windLayer.BaseAltitudeFt,
            windLayer.TopAltitudeFt
        );
    }

    /// <summary>
    /// Gets the appropriate Wind object for an aircraft's altitude
    /// </summary>
    public static Wind? GetWindForAircraft(this WeatherConditions weather, AircraftModel aircraft)
    {
        var windLayer = weather.GetWindAtAltitude(aircraft.AltitudeFt);
        return windLayer?.ToWind();
    }

    /// <summary>
    /// Applies weather to an aircraft (sets wind)
    /// </summary>
    public static void ApplyToAircraft(this WeatherConditions weather, AircraftModel aircraft)
    {
        var wind = weather.GetWindForAircraft(aircraft);
        aircraft.SetWind(wind);
    }
}
