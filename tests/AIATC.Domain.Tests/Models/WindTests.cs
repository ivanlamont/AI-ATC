using Xunit;
using AIATC.Domain.Models;
using System;

namespace AIATC.Domain.Tests.Models;

public class WindTests
{
    [Fact]
    public void Wind_CalculatesCrosswindComponent()
    {
        // Wind from 090 (east) at 20 knots
        var wind = new Wind(90, 20);

        // Runway heading 360 (north)
        var crosswind = wind.GetCrosswindComponent(360);

        // Full 90-degree crosswind = full wind speed
        Assert.Equal(20, crosswind, 1);
    }

    [Fact]
    public void Wind_CalculatesHeadwindComponent()
    {
        // Wind from 360 (north) at 30 knots
        var wind = new Wind(360, 30);

        // Runway heading 180 (south) - direct tailwind
        var headwind = wind.GetHeadwindComponent(180);

        Assert.True(headwind < 0); // Negative = tailwind
        Assert.Equal(-30, headwind, 1);
    }

    [Fact]
    public void Wind_DirectHeadwind()
    {
        // Wind from 360 (north) at 25 knots
        var wind = new Wind(360, 25);

        // Runway heading 360 (north) - direct headwind
        var headwind = wind.GetHeadwindComponent(360);

        Assert.Equal(25, headwind, 1);
    }

    [Fact]
    public void Wind_GetWindVelocityVector()
    {
        // Wind from 270 (west, blowing TO east) at 30 knots
        var wind = new Wind(270, 30);

        var velocity = wind.GetWindVelocityNmPerSec();

        // Should point east (positive X)
        Assert.True(velocity.X > 0);
        Assert.Equal(0, velocity.Y, 3);
    }

    [Fact]
    public void CalmWind_HasNoEffect()
    {
        var wind = Wind.Calm;

        Assert.Equal(0, wind.SpeedKnots);
        var velocity = wind.GetWindVelocityNmPerSec();
        Assert.Equal(0, velocity.X);
        Assert.Equal(0, velocity.Y);
    }
}
