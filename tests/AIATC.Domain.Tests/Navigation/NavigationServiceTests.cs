using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Commands;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Tests.Navigation;

public class NavigationServiceTests
{
    [Fact]
    public void ProcessDirectCommand_ValidFix_ReturnsHeading()
    {
        var db = new NavigationDatabase();
        db.AddFix(new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(0, 10) // North of origin
        });

        var service = new NavigationService(db);
        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var command = new DirectCommand { FixName = "BEBOP" };
        var (success, heading, error) = service.ProcessDirectCommand(command, aircraft);

        Assert.True(success);
        Assert.NotNull(heading);
        Assert.Equal(0f, heading.Value, 1); // Should be heading north
        Assert.Null(error);
    }

    [Fact]
    public void ProcessDirectCommand_InvalidFix_ReturnsError()
    {
        var db = new NavigationDatabase();
        var service = new NavigationService(db);
        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var command = new DirectCommand { FixName = "NOTHERE" };
        var (success, heading, error) = service.ProcessDirectCommand(command, aircraft);

        Assert.False(success);
        Assert.Null(heading);
        Assert.NotNull(error);
        Assert.Contains("not found", error);
    }

    [Fact]
    public void ProcessApproachCommand_ValidApproach_ReturnsProcedure()
    {
        var db = new NavigationDatabase();
        var procedure = new Procedure
        {
            Identifier = "ILS27",
            Type = ProcedureType.Approach,
            AirportIdentifier = "KSFO",
            RunwayIdentifier = "27"
        };
        db.AddProcedure(procedure);

        var service = new NavigationService(db);
        var command = new ApproachCommand
        {
            Type = ApproachType.ILS,
            RunwayIdentifier = "27"
        };

        var (success, proc, error) = service.ProcessApproachCommand(command, "KSFO");

        Assert.True(success);
        Assert.NotNull(proc);
        Assert.Equal("ILS27", proc.Identifier);
        Assert.Null(error);
    }

    [Fact]
    public void ProcessApproachCommand_InvalidApproach_ReturnsError()
    {
        var db = new NavigationDatabase();
        var service = new NavigationService(db);
        var command = new ApproachCommand
        {
            Type = ApproachType.ILS,
            RunwayIdentifier = "27"
        };

        var (success, proc, error) = service.ProcessApproachCommand(command, "KSFO");

        Assert.False(success);
        Assert.Null(proc);
        Assert.NotNull(error);
        Assert.Contains("found", error.ToLower());
    }

    [Fact]
    public void ProcessHoldCommand_ValidFix_ReturnsPattern()
    {
        var db = new NavigationDatabase();
        db.AddFix(new Fix
        {
            Identifier = "SUNST",
            PositionNm = new Vector2(10, 10)
        });

        var service = new NavigationService(db);
        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0),
            HeadingDegrees = 90
        };

        var command = new HoldCommand
        {
            FixName = "SUNST",
            InboundCourseDegrees = 270,
            TurnDirection = TurnDirection.Right
        };

        var (success, pattern, error) = service.ProcessHoldCommand(command, aircraft);

        Assert.True(success);
        Assert.NotNull(pattern);
        Assert.Equal("SUNST", pattern.Fix.Identifier);
        Assert.Equal(270f, pattern.InboundCourseDegrees);
        Assert.Null(error);
    }

    [Fact]
    public void ProcessHoldCommand_InvalidFix_ReturnsError()
    {
        var db = new NavigationDatabase();
        var service = new NavigationService(db);
        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 0)
        };

        var command = new HoldCommand
        {
            FixName = "NOTHERE",
            TurnDirection = TurnDirection.Right
        };

        var (success, pattern, error) = service.ProcessHoldCommand(command, aircraft);

        Assert.False(success);
        Assert.Null(pattern);
        Assert.NotNull(error);
        Assert.Contains("not found", error);
    }

    [Fact]
    public void FollowRoute_UpdatesAircraftHeading()
    {
        var db = new NavigationDatabase();
        var service = new NavigationService(db);

        var route = new Route();
        route.AddFix(new Fix { Identifier = "FIX1", PositionNm = new Vector2(0, 0) });
        route.AddFix(new Fix { Identifier = "FIX2", PositionNm = new Vector2(0, 10) }); // North

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(0, 5),
            HeadingDegrees = 90 // Heading east
        };

        service.FollowRoute(aircraft, route);

        // Should have updated target heading to point north
        Assert.NotNull(aircraft.TargetHeadingDegrees);
        Assert.Equal(0f, aircraft.TargetHeadingDegrees.Value, 1);
        Assert.NotEqual(0f, aircraft.TurnRateRadPerSec); // Should be turning
    }
}
