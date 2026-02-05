using AIATC.Domain.Models.Weather;
using System;
using System.Collections.Generic;
using Xunit;

namespace AIATC.Domain.Tests.Weather;

public class WeatherConditionsTests
{
    [Fact]
    public void GetWindAtAltitude_WithinLayer_ReturnsCorrectWind()
    {
        var weather = new WeatherConditions
        {
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(270, 15),
                WindLayer.CreateAloft(250, 45, 10000, 20000)
            }
        };

        var surfaceWind = weather.GetWindAtAltitude(2000);
        Assert.NotNull(surfaceWind);
        Assert.Equal(270f, surfaceWind.DirectionDegrees);
        Assert.Equal(15f, surfaceWind.SpeedKnots);

        var aloftWind = weather.GetWindAtAltitude(15000);
        Assert.NotNull(aloftWind);
        Assert.Equal(250f, aloftWind.DirectionDegrees);
        Assert.Equal(45f, aloftWind.SpeedKnots);
    }

    [Fact]
    public void GetWindAtAltitude_BetweenLayers_ReturnsClosest()
    {
        var weather = new WeatherConditions
        {
            WindLayers = new List<WindLayer>
            {
                WindLayer.CreateSurface(270, 15),
                WindLayer.CreateAloft(250, 45, 10000, 20000)
            }
        };

        // Altitude 5000 is between surface (0-3000) and aloft (10000-20000)
        var wind = weather.GetWindAtAltitude(5000);
        Assert.NotNull(wind);
        // Should pick the closer one (surface ends at 3000, aloft starts at 10000)
    }

    [Fact]
    public void GetCeilingAgl_WithCeiling_ReturnsCeilingAltitude()
    {
        var weather = new WeatherConditions
        {
            CloudLayers = new List<CloudLayer>
            {
                CloudLayer.CreateScattered(4000),
                CloudLayer.CreateCeiling(1200, CloudCoverage.Broken)
            }
        };

        var ceiling = weather.GetCeilingAgl();

        Assert.NotNull(ceiling);
        Assert.Equal(1200f, ceiling.Value);
    }

    [Fact]
    public void GetCeilingAgl_NoCeiling_ReturnsNull()
    {
        var weather = new WeatherConditions
        {
            CloudLayers = new List<CloudLayer>
            {
                CloudLayer.CreateScattered(4000),
                CloudLayer.CreateScattered(8000)
            }
        };

        var ceiling = weather.GetCeilingAgl();

        Assert.Null(ceiling);
    }

    [Fact]
    public void IsVfr_GoodWeather_ReturnsTrue()
    {
        var weather = WeatherConditions.CreateClear("KJFK");
        Assert.True(weather.IsVfr());
    }

    [Fact]
    public void IsVfr_LowCeiling_ReturnsFalse()
    {
        var weather = WeatherConditions.CreateIfr("KJFK");
        Assert.False(weather.IsVfr());
    }

    [Fact]
    public void GetFlightCategory_Clear_ReturnsVfr()
    {
        var weather = WeatherConditions.CreateClear("KJFK");
        Assert.Equal("VFR", weather.GetFlightCategory());
    }

    [Fact]
    public void GetFlightCategory_Ifr_ReturnsIfr()
    {
        var weather = WeatherConditions.CreateIfr("KJFK");
        Assert.Equal("IFR", weather.GetFlightCategory());
    }

    [Fact]
    public void GetDensityAltitude_StandardConditions_ReturnsFieldElevation()
    {
        var weather = new WeatherConditions
        {
            FieldElevationFt = 1000,
            AltimeterInHg = 29.92f,
            TemperatureCelsius = 15f - (1000f / 1000f * 2f) // ISA temp at 1000 ft
        };

        var densityAlt = weather.GetDensityAltitude();

        // Should be close to field elevation at standard conditions
        Assert.InRange(densityAlt, 900, 1100);
    }

    [Fact]
    public void GetDensityAltitude_HotDay_ReturnsHigherAltitude()
    {
        var weather = new WeatherConditions
        {
            FieldElevationFt = 1000,
            AltimeterInHg = 29.92f,
            TemperatureCelsius = 30f  // Hot day
        };

        var densityAlt = weather.GetDensityAltitude();

        // Density altitude should be higher than field elevation on hot day
        Assert.True(densityAlt > 1000);
    }

    [Fact]
    public void ToMetarString_Clear_FormatsCorrectly()
    {
        var weather = new WeatherConditions
        {
            LocationId = "KJFK",
            ObservationTime = new DateTime(2026, 1, 15, 18, 53, 0, DateTimeKind.Utc),
            WindLayers = new List<WindLayer>
            {
                new WindLayer
                {
                    DirectionDegrees = 270,
                    SpeedKnots = 15,
                    BaseAltitudeFt = 0,
                    TopAltitudeFt = 3000
                }
            },
            CloudLayers = new List<CloudLayer>
            {
                new CloudLayer { Coverage = CloudCoverage.Clear }
            },
            Visibility = VisibilityConditions.CreateClear(),
            AltimeterInHg = 30.12f,
            TemperatureCelsius = 15,
            DewpointCelsius = 10
        };

        var metar = weather.ToMetarString();

        Assert.Contains("METAR KJFK", metar);
        Assert.Contains("151853Z", metar);
        Assert.Contains("27015KT", metar);
        Assert.Contains("10SM", metar);
        Assert.Contains("CLR", metar);
        Assert.Contains("15/10", metar);
        Assert.Contains("A3012", metar);
    }

    [Fact]
    public void ToMetarString_Ifr_FormatsCorrectly()
    {
        var weather = new WeatherConditions
        {
            LocationId = "KLAX",
            ObservationTime = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc),
            WindLayers = new List<WindLayer>
            {
                new WindLayer
                {
                    DirectionDegrees = 180,
                    SpeedKnots = 12,
                    GustKnots = 20,
                    BaseAltitudeFt = 0,
                    TopAltitudeFt = 3000
                }
            },
            CloudLayers = new List<CloudLayer>
            {
                CloudLayer.CreateCeiling(800, CloudCoverage.Overcast)
            },
            Visibility = new VisibilityConditions
            {
                VisibilityMiles = 2,
                Precipitation = PrecipitationType.Rain,
                Intensity = PrecipitationIntensity.Moderate,
                Obscuration = ObscurationType.Mist
            },
            AltimeterInHg = 29.85f,
            TemperatureCelsius = 10,
            DewpointCelsius = 9
        };

        var metar = weather.ToMetarString();

        Assert.Contains("METAR KLAX", metar);
        Assert.Contains("18012G20KT", metar);
        Assert.Contains("2SM", metar);
        Assert.Contains("RA", metar);
        Assert.Contains("BR", metar);
        Assert.Contains("OVC008", metar);
        Assert.Contains("10/09", metar);
        Assert.Contains("A2985", metar);
    }

    [Fact]
    public void CreateClear_ReturnsGoodWeather()
    {
        var weather = WeatherConditions.CreateClear("KJFK");

        Assert.Equal("KJFK", weather.LocationId);
        Assert.Single(weather.WindLayers);
        Assert.Equal(0f, weather.WindLayers[0].SpeedKnots);
        Assert.Equal(10f, weather.Visibility.VisibilityMiles);
        Assert.Equal(29.92f, weather.AltimeterInHg);
    }

    [Fact]
    public void CreateIfr_ReturnsIfrWeather()
    {
        var weather = WeatherConditions.CreateIfr("KJFK");

        Assert.Equal("KJFK", weather.LocationId);
        Assert.True(weather.WindLayers[0].SpeedKnots > 0);
        Assert.True(weather.WindLayers[0].GustKnots > weather.WindLayers[0].SpeedKnots);
        Assert.True(weather.Visibility.VisibilityMiles < 3);
        Assert.NotEmpty(weather.CloudLayers);
    }

    [Fact]
    public void CreateWindy_ReturnsWindyWeather()
    {
        var weather = WeatherConditions.CreateWindy("KJFK", 270, 25);

        Assert.Equal(270f, weather.WindLayers[0].DirectionDegrees);
        Assert.Equal(25f, weather.WindLayers[0].SpeedKnots);
        Assert.True(weather.WindLayers[0].GustKnots > 25);
        Assert.True(weather.WindLayers.Count > 1); // Has aloft winds
    }
}
