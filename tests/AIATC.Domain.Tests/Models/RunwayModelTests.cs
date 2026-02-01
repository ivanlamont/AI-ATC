using Xunit;
using AIATC.Domain.Models;
using System;

namespace AIATC.Domain.Tests.Models;

public class RunwayModelTests
{
    [Fact]
    public void Runway_CalculatesLocalizerDirection()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var runway = new RunwayModel(airport, "27", 270); // West

        var locDir = runway.LocalizerDirection;

        Assert.True(locDir.X < 0); // Points west (negative X)
        Assert.Equal(0, locDir.Y, 2); // No north/south component
    }

    [Fact]
    public void Runway_CalculatesFafPosition()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var runway = new RunwayModel(airport, "09", 90, fafDistanceNm: 5.0f); // East, FAF at 5 NM

        var fafPos = runway.GetFafPosition();

        // FAF should be 5 NM west of airport (outbound from runway 09)
        Assert.Equal(-5, fafPos.X, 1);
        Assert.Equal(0, fafPos.Y, 1);
    }

    [Fact]
    public void Runway_CalculatesGlideslopeAltitude()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0),
            AltitudeFt = 0
        };

        var runway = new RunwayModel(airport, "27", 270);

        // At 6 NM from threshold, standard 3-degree glideslope
        var altitude = runway.GetGlideslopeAltitude(6.0f);

        // 3-degree glideslope ≈ 318 ft/nm
        // 6 NM * 318 ≈ 1908 ft
        Assert.InRange(altitude, 1800, 2000);
    }

    [Fact]
    public void Runway_CalculatesDistanceAlongLocalizer()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var runway = new RunwayModel(airport, "09", 90); // East

        // Aircraft 10 NM west of airport (on extended centerline)
        var position = new Vector2(-10, 0);

        var distance = runway.GetDistanceAlongLocalizer(position);

        Assert.Equal(10, distance, 1);
    }

    [Fact]
    public void Runway_CalculatesLocalizerDeviation()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var runway = new RunwayModel(airport, "36", 360); // North

        // Aircraft 1 NM east of centerline (right of course)
        var position = new Vector2(1, 5);

        var deviation = runway.GetDeviationFromLocalizer(position);

        Assert.True(deviation > 0); // Right of course
        Assert.Equal(1, deviation, 1);
    }
}
