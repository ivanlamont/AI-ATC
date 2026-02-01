using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Tests.Navigation;

public class FixTests
{
    [Fact]
    public void GetDistanceNm_CalculatesCorrectDistance()
    {
        var fix = new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(10, 10)
        };

        var position = new Vector2(13, 14);
        var distance = fix.GetDistanceNm(position);

        // Distance should be sqrt((13-10)^2 + (14-10)^2) = sqrt(9+16) = 5
        Assert.Equal(5.0f, distance, 2);
    }

    [Fact]
    public void GetBearingTo_CalculatesCorrectBearing()
    {
        var fix = new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(0, 0)
        };

        // Position due north
        var northPosition = new Vector2(0, 10);
        var bearing = fix.GetBearingTo(northPosition);
        Assert.Equal(0f, bearing, 1);

        // Position due east
        var eastPosition = new Vector2(10, 0);
        bearing = fix.GetBearingTo(eastPosition);
        Assert.Equal(90f, bearing, 1);

        // Position due south
        var southPosition = new Vector2(0, -10);
        bearing = fix.GetBearingTo(southPosition);
        Assert.Equal(180f, bearing, 1);

        // Position due west
        var westPosition = new Vector2(-10, 0);
        bearing = fix.GetBearingTo(westPosition);
        Assert.Equal(270f, bearing, 1);
    }

    [Fact]
    public void GetBearingTo_NormalizesBearingTo360()
    {
        var fix = new Fix
        {
            Identifier = "TEST",
            PositionNm = new Vector2(0, 0)
        };

        var position = new Vector2(5, 5); // Northeast
        var bearing = fix.GetBearingTo(position);

        Assert.True(bearing >= 0 && bearing < 360);
        Assert.Equal(45f, bearing, 1);
    }
}
