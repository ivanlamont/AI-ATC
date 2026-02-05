using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Commands;

namespace AIATC.Domain.Tests.Commands;

public class ClearanceApplicatorTests
{
    private readonly ClearanceApplicator _applicator = new();

    [Fact]
    public void ApplyHeadingClearance_SetsTargetHeading()
    {
        var aircraft = new AircraftModel
        {
            HeadingDegrees = 360,
            SpeedKnots = 200,
            AltitudeFt = 5000
        };

        var command = new HeadingCommand
        {
            TargetHeadingDegrees = 90,
            Direction = TurnDirection.Right
        };

        var result = _applicator.ApplyCommand(command, aircraft);

        Assert.True(result);
        Assert.Equal(90, aircraft.TargetHeadingDegrees);
        Assert.True(aircraft.TurnRateRadPerSec > 0); // Turning right
    }

    [Fact]
    public void ApplyAltitudeClearance_SetsTargetAltitude()
    {
        var aircraft = new AircraftModel
        {
            AltitudeFt = 10000,
            SpeedKnots = 200
        };

        var command = new AltitudeCommand
        {
            TargetAltitudeFeet = 4000,
            ChangeType = AltitudeChange.Descend
        };

        var result = _applicator.ApplyCommand(command, aircraft);

        Assert.True(result);
        Assert.Equal(4000, aircraft.TargetAltitudeFt);
        Assert.True(aircraft.VerticalSpeedFpm < 0); // Descending
    }

    [Fact]
    public void ApplySpeedClearance_SetsTargetSpeed()
    {
        var aircraft = new AircraftModel
        {
            SpeedKnots = 220,
            MinSpeedKnots = 160,
            MaxSpeedKnots = 260
        };

        var command = new SpeedCommand
        {
            TargetSpeedKnots = 180,
            ChangeType = SpeedChange.Reduce
        };

        var result = _applicator.ApplyCommand(command, aircraft);

        Assert.True(result);
        Assert.Equal(180, aircraft.TargetSpeedKnots);
        Assert.True(aircraft.AccelerationKnotsPerSec < 0); // Decelerating
    }

    [Fact]
    public void ValidateCommand_RejectsInvalidHeading()
    {
        var aircraft = new AircraftModel();
        var command = new HeadingCommand { TargetHeadingDegrees = 400 };

        var (valid, error) = _applicator.ValidateCommand(command, aircraft);

        Assert.False(valid);
        Assert.NotNull(error);
        Assert.Contains("heading", error.ToLower());
    }

    [Fact]
    public void ValidateCommand_RejectsInvalidAltitude()
    {
        var aircraft = new AircraftModel();
        var command = new AltitudeCommand { TargetAltitudeFeet = -1000 };

        var (valid, error) = _applicator.ValidateCommand(command, aircraft);

        Assert.False(valid);
        Assert.NotNull(error);
        Assert.Contains("altitude", error.ToLower());
    }

    [Fact]
    public void ValidateCommand_RejectsSpeedBelowMinimum()
    {
        var aircraft = new AircraftModel
        {
            MinSpeedKnots = 160,
            MaxSpeedKnots = 260
        };
        var command = new SpeedCommand { TargetSpeedKnots = 100 };

        var (valid, error) = _applicator.ValidateCommand(command, aircraft);

        Assert.False(valid);
        Assert.NotNull(error);
        Assert.Contains("minimum", error.ToLower());
    }

    [Fact]
    public void ValidateCommand_RejectsSpeedAboveMaximum()
    {
        var aircraft = new AircraftModel
        {
            MinSpeedKnots = 160,
            MaxSpeedKnots = 260
        };
        var command = new SpeedCommand { TargetSpeedKnots = 300 };

        var (valid, error) = _applicator.ValidateCommand(command, aircraft);

        Assert.False(valid);
        Assert.NotNull(error);
        Assert.Contains("maximum", error.ToLower());
    }

    [Fact]
    public void ApplyHeadingClearance_CalculatesShortestTurn()
    {
        var aircraft = new AircraftModel
        {
            HeadingDegrees = 10
        };

        var command = new HeadingCommand
        {
            TargetHeadingDegrees = 350,
            Direction = TurnDirection.Either // Should turn left (shortest)
        };

        _applicator.ApplyCommand(command, aircraft);

        Assert.True(aircraft.TurnRateRadPerSec < 0); // Turning left
    }

    [Fact]
    public void ApplyAltitudeClearance_StopsAtTarget()
    {
        var aircraft = new AircraftModel
        {
            AltitudeFt = 5000
        };

        var command = new AltitudeCommand
        {
            TargetAltitudeFeet = 5000, // Already at target
            ChangeType = AltitudeChange.Maintain
        };

        _applicator.ApplyCommand(command, aircraft);

        Assert.Equal(0, aircraft.VerticalSpeedFpm); // Should not be climbing or descending
    }

    [Fact]
    public void ApplyContactClearance_ReturnsSuccess()
    {
        var aircraft = new AircraftModel();
        var command = new ContactCommand
        {
            FacilityName = "Tower",
            FrequencyMhz = 120.5f
        };

        var result = _applicator.ApplyCommand(command, aircraft);

        Assert.True(result); // Contact clearance doesn't affect aircraft controls
    }
}
