using AIATC.Domain.Models;
using AIATC.Domain.Models.Weather;
using System;
using Xunit;

namespace AIATC.Domain.Tests.Weather;

public class WindLayerTests
{
    [Fact]
    public void GetWindVector_NorthWind_ReturnsCorrectVector()
    {
        // Wind FROM north (360°) blows TO south
        var wind = new WindLayer
        {
            DirectionDegrees = 360,
            SpeedKnots = 10
        };

        var vector = wind.GetWindVector();

        // Wind should point north (positive Y)
        Assert.True(Math.Abs(vector.X) < 0.1f);
        Assert.True(vector.Y > 9.9f && vector.Y < 10.1f);
    }

    [Fact]
    public void GetWindVector_EastWind_ReturnsCorrectVector()
    {
        // Wind FROM east (90°) blows TO west
        var wind = new WindLayer
        {
            DirectionDegrees = 90,
            SpeedKnots = 15
        };

        var vector = wind.GetWindVector();

        // Wind should point east (positive X)
        Assert.True(vector.X > 14.9f && vector.X < 15.1f);
        Assert.True(Math.Abs(vector.Y) < 0.1f);
    }

    [Fact]
    public void GetCurrentSpeed_NoGusts_ReturnsSustained()
    {
        var wind = new WindLayer
        {
            SpeedKnots = 10,
            GustKnots = 0
        };

        for (int i = 0; i < 100; i++)
        {
            var speed = wind.GetCurrentSpeed();
            Assert.Equal(10f, speed);
        }
    }

    [Fact]
    public void GetCurrentSpeed_WithGusts_SometimesGusts()
    {
        var wind = new WindLayer
        {
            SpeedKnots = 10,
            GustKnots = 20
        };

        var hasGust = false;
        var hasSustained = false;

        for (int i = 0; i < 1000; i++)
        {
            var speed = wind.GetCurrentSpeed();

            if (speed > 10.5f)
                hasGust = true;
            if (speed < 10.5f)
                hasSustained = true;
        }

        Assert.True(hasGust, "Should have gusts");
        Assert.True(hasSustained, "Should have sustained wind");
    }

    [Fact]
    public void ContainsAltitude_WithinRange_ReturnsTrue()
    {
        var wind = WindLayer.CreateSurface(270, 15);

        Assert.True(wind.ContainsAltitude(0));
        Assert.True(wind.ContainsAltitude(1500));
        Assert.True(wind.ContainsAltitude(3000));
    }

    [Fact]
    public void ContainsAltitude_OutsideRange_ReturnsFalse()
    {
        var wind = WindLayer.CreateSurface(270, 15);

        Assert.False(wind.ContainsAltitude(-1));
        Assert.False(wind.ContainsAltitude(3001));
        Assert.False(wind.ContainsAltitude(10000));
    }

    [Fact]
    public void CreateSurface_SetsCorrectAltitudeRange()
    {
        var wind = WindLayer.CreateSurface(180, 12, 18);

        Assert.Equal(180f, wind.DirectionDegrees);
        Assert.Equal(12f, wind.SpeedKnots);
        Assert.Equal(18f, wind.GustKnots);
        Assert.Equal(0f, wind.BaseAltitudeFt);
        Assert.Equal(3000f, wind.TopAltitudeFt);
    }

    [Fact]
    public void CreateAloft_SetsCorrectParameters()
    {
        var wind = WindLayer.CreateAloft(250, 45, 10000, 20000);

        Assert.Equal(250f, wind.DirectionDegrees);
        Assert.Equal(45f, wind.SpeedKnots);
        Assert.Equal(0f, wind.GustKnots);  // No gusts aloft
        Assert.Equal(10000f, wind.BaseAltitudeFt);
        Assert.Equal(20000f, wind.TopAltitudeFt);
    }

    [Fact]
    public void ToMetarString_NoGusts_FormatsCorrectly()
    {
        var wind = new WindLayer
        {
            DirectionDegrees = 270,
            SpeedKnots = 15
        };

        Assert.Equal("27015KT", wind.ToMetarString());
    }

    [Fact]
    public void ToMetarString_WithGusts_FormatsCorrectly()
    {
        var wind = new WindLayer
        {
            DirectionDegrees = 180,
            SpeedKnots = 12,
            GustKnots = 22
        };

        Assert.Equal("18012G22KT", wind.ToMetarString());
    }

    [Fact]
    public void ToMetarString_VariableWind_FormatsCorrectly()
    {
        var wind = new WindLayer
        {
            DirectionDegrees = 5,  // 005 degrees
            SpeedKnots = 3
        };

        Assert.Equal("00503KT", wind.ToMetarString());
    }
}
