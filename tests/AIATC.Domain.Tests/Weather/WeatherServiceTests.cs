using AIATC.Domain.Models.Weather;
using AIATC.Domain.Services;
using Xunit;

namespace AIATC.Domain.Tests.Weather;

public class WeatherServiceTests
{
    [Fact]
    public void GetWeather_NoWeatherSet_ReturnsDefaultClear()
    {
        var service = new WeatherService();

        var weather = service.GetWeather("KJFK");

        Assert.NotNull(weather);
        Assert.Equal("KJFK", weather.LocationId);
        Assert.True(weather.IsVfr());
    }

    [Fact]
    public void SetWeather_StoresWeather()
    {
        var service = new WeatherService();
        var weather = WeatherConditions.CreateIfr("KLAX");

        service.SetWeather("KLAX", weather);

        var retrieved = service.GetWeather("KLAX");
        Assert.Equal("KLAX", retrieved.LocationId);
        Assert.False(retrieved.IsVfr());
    }

    [Fact]
    public void SetWeather_RaisesWeatherChangedEvent()
    {
        var service = new WeatherService();
        var eventRaised = false;
        string? changedLocation = null;

        service.WeatherChanged += (sender, args) =>
        {
            eventRaised = true;
            changedLocation = args.LocationId;
        };

        var weather = WeatherConditions.CreateClear("KJFK");
        service.SetWeather("KJFK", weather);

        Assert.True(eventRaised);
        Assert.Equal("KJFK", changedLocation);
    }

    [Fact]
    public void GenerateRandomWeather_Easy_ReturnsLightConditions()
    {
        var service = new WeatherService();

        var weather = service.GenerateRandomWeather("KJFK", WeatherDifficulty.Easy);

        Assert.NotNull(weather);
        Assert.True(weather.WindLayers[0].SpeedKnots < 15);
        Assert.True(weather.Visibility.VisibilityMiles >= 5);
    }

    [Fact]
    public void GenerateRandomWeather_Medium_ReturnsModerateConditions()
    {
        var service = new WeatherService();

        var weather = service.GenerateRandomWeather("KJFK", WeatherDifficulty.Medium);

        Assert.NotNull(weather);
        Assert.True(weather.WindLayers[0].SpeedKnots >= 10);
        Assert.True(weather.WindLayers[0].SpeedKnots < 25);
    }

    [Fact]
    public void GenerateRandomWeather_Hard_ReturnsChallengingConditions()
    {
        var service = new WeatherService();

        var weather = service.GenerateRandomWeather("KJFK", WeatherDifficulty.Hard);

        Assert.NotNull(weather);
        Assert.True(weather.WindLayers[0].SpeedKnots >= 18);
        var ceiling = weather.GetCeilingAgl();
        Assert.NotNull(ceiling);
        Assert.True(ceiling.Value < 2000);
    }

    [Fact]
    public void GenerateRandomWeather_Extreme_ReturnsVeryDifficultConditions()
    {
        var service = new WeatherService();

        var weather = service.GenerateRandomWeather("KJFK", WeatherDifficulty.Extreme);

        Assert.NotNull(weather);
        Assert.True(weather.WindLayers[0].SpeedKnots >= 25);
        Assert.True(weather.Visibility.VisibilityMiles < 2);
        var ceiling = weather.GetCeilingAgl();
        Assert.NotNull(ceiling);
        Assert.True(ceiling.Value < 600);
    }

    [Fact]
    public void UpdateWeather_ModifiesWindGradually()
    {
        var service = new WeatherService();
        var weather = WeatherConditions.CreateClear("KJFK");
        weather.WindLayers[0].DirectionDegrees = 270;
        weather.WindLayers[0].SpeedKnots = 10;

        service.SetWeather("KJFK", weather);

        // Update many times
        for (int i = 0; i < 100; i++)
        {
            service.UpdateWeather("KJFK", 1.0f);
        }

        var updated = service.GetWeather("KJFK");

        // Wind should have varied slightly (but not drastically)
        Assert.InRange(updated.WindLayers[0].DirectionDegrees, 200, 340);
        Assert.InRange(updated.WindLayers[0].SpeedKnots, 0, 20);
    }

    [Fact]
    public void GetLocations_ReturnsAllLocations()
    {
        var service = new WeatherService();

        service.SetWeather("KJFK", WeatherConditions.CreateClear("KJFK"));
        service.SetWeather("KLAX", WeatherConditions.CreateClear("KLAX"));
        service.SetWeather("KORD", WeatherConditions.CreateClear("KORD"));

        var locations = service.GetLocations();

        Assert.Contains("KJFK", locations);
        Assert.Contains("KLAX", locations);
        Assert.Contains("KORD", locations);
    }

    [Fact]
    public void Clear_RemovesAllWeather()
    {
        var service = new WeatherService();

        service.SetWeather("KJFK", WeatherConditions.CreateClear("KJFK"));
        service.SetWeather("KLAX", WeatherConditions.CreateClear("KLAX"));

        service.Clear();

        var locations = service.GetLocations();
        Assert.Empty(locations);
    }
}
