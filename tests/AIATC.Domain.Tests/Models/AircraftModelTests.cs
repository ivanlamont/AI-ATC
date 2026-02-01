using Xunit;
using AIATC.Domain.Models;
using System;

namespace AIATC.Domain.Tests.Models;

public class AircraftModelTests
{
    [Fact]
    public void Aircraft_InitializesWithCorrectDefaults()
    {
        var aircraft = new AircraftModel();

        Assert.Equal(0, aircraft.PositionNm.X);
        Assert.Equal(0, aircraft.PositionNm.Y);
        Assert.Equal(220, aircraft.SpeedKnots);
        Assert.Equal(5000, aircraft.AltitudeFt);
        Assert.False(aircraft.Landed);
    }

    [Fact]
    public void Aircraft_MovesInHeadingDirection()
    {
        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0),
            HeadingDegrees = 90, // East in aviation (90° = East)
            SpeedKnots = 180, // 3 NM/min = 0.05 NM/sec
            AltitudeFt = 5000
        };

        // Step for 1 second
        aircraft.Step(1.0f);

        // Should move 0.05 NM east (180 knots = 180/3600 = 0.05 NM/sec)
        Assert.True(aircraft.PositionNm.X > 0);
        Assert.Equal(0, aircraft.PositionNm.Y, 2);
    }

    [Fact]
    public void Aircraft_TurnsCorrectly()
    {
        var aircraft = new AircraftModel
        {
            HeadingRadians = 0,
            TurnRateRadPerSec = 3.0f * SimulationConstants.DegreesToRadians
        };

        aircraft.Step(1.0f);

        var expectedHeading = 3.0f * SimulationConstants.DegreesToRadians;
        Assert.Equal(expectedHeading, aircraft.HeadingRadians, 3);
    }

    [Fact]
    public void Aircraft_ChangesAltitude()
    {
        var aircraft = new AircraftModel
        {
            AltitudeFt = 5000,
            VerticalSpeedFpm = 1000 // Climbing at 1000 ft/min
        };

        aircraft.Step(60.0f); // 1 minute

        Assert.Equal(6000, aircraft.AltitudeFt, 1);
    }

    [Fact]
    public void Aircraft_ClampsSpeedToLimits()
    {
        var aircraft = new AircraftModel
        {
            SpeedKnots = 200,
            MinSpeedKnots = 160,
            MaxSpeedKnots = 260,
            AccelerationKnotsPerSec = 10
        };

        aircraft.Step(10.0f); // Try to accelerate 100 knots

        Assert.Equal(260, aircraft.SpeedKnots); // Should clamp to max
    }

    [Fact]
    public void Aircraft_CalculatesDistanceToDestination()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(10, 0)
        };

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0),
            Destination = airport
        };

        var distance = aircraft.GetDistanceToDestinationNm();

        Assert.Equal(10, distance, 2);
    }

    [Fact]
    public void Aircraft_DetectsLanding()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0),
            AltitudeFt = 0
        };

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0.5f, 0), // 0.5 NM from airport
            AltitudeFt = 500,
            SpeedKnots = 140,
            VerticalSpeedFpm = -500,
            TurnRateRadPerSec = 0
        };

        bool landed = aircraft.CheckLanding(airport, 2.0f);

        Assert.True(landed);
        Assert.True(aircraft.Landed);
    }

    [Fact]
    public void Aircraft_DoesNotLandWhenTooFast()
    {
        var airport = new AirportModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0.5f, 0),
            AltitudeFt = 500,
            SpeedKnots = 200, // Too fast!
            VerticalSpeedFpm = -500
        };

        bool landed = aircraft.CheckLanding(airport, 2.0f);

        Assert.False(landed);
    }

    [Fact]
    public void Aircraft_AffectedByWind()
    {
        // Aircraft heading east at 180 knots
        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0),
            HeadingDegrees = 90, // East in aviation
            SpeedKnots = 180
        };

        // Wind from north (0°, blowing south) at 30 knots
        var wind = new Wind(360, 30);
        aircraft.SetWind(wind);

        aircraft.Step(60.0f); // 1 minute

        // Should drift south due to wind
        Assert.True(aircraft.PositionNm.X > 0); // Still moved east
        Assert.True(aircraft.PositionNm.Y < 0); // Drifted south
    }

    [Fact]
    public void Aircraft_GroundSpeedDifferentFromAirspeed()
    {
        var aircraft = new AircraftModel
        {
            HeadingDegrees = 90, // East
            SpeedKnots = 180 // Airspeed
        };

        // Tailwind from west (270°, blowing east) at 30 knots
        var wind = new Wind(270, 30);
        aircraft.SetWind(wind);

        var groundSpeed = aircraft.GetGroundSpeedKnots();

        Assert.True(groundSpeed > 180); // Ground speed should be higher with tailwind
        Assert.InRange(groundSpeed, 205, 215); // ~210 knots
    }
}
