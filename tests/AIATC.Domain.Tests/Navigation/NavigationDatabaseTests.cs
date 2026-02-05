using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Tests.Navigation;

public class NavigationDatabaseTests
{
    [Fact]
    public void AddFix_IncreasesFixCount()
    {
        var db = new NavigationDatabase();
        var fix = new Fix { Identifier = "BEBOP", PositionNm = new Vector2(10, 10) };

        db.AddFix(fix);

        Assert.Equal(1, db.FixCount);
    }

    [Fact]
    public void GetFix_ReturnsAddedFix()
    {
        var db = new NavigationDatabase();
        var fix = new Fix { Identifier = "BEBOP", PositionNm = new Vector2(10, 10) };

        db.AddFix(fix);
        var retrieved = db.GetFix("BEBOP");

        Assert.NotNull(retrieved);
        Assert.Equal("BEBOP", retrieved.Identifier);
    }

    [Fact]
    public void GetFix_IsCaseInsensitive()
    {
        var db = new NavigationDatabase();
        var fix = new Fix { Identifier = "BEBOP", PositionNm = new Vector2(10, 10) };

        db.AddFix(fix);
        var retrieved = db.GetFix("bebop");

        Assert.NotNull(retrieved);
        Assert.Equal("BEBOP", retrieved.Identifier);
    }

    [Fact]
    public void GetFix_NonExistent_ReturnsNull()
    {
        var db = new NavigationDatabase();
        var retrieved = db.GetFix("NOTHERE");

        Assert.Null(retrieved);
    }

    [Fact]
    public void GetFixesNear_ReturnsFixesWithinRadius()
    {
        var db = new NavigationDatabase();
        db.AddFix(new Fix { Identifier = "FIX1", PositionNm = new Vector2(0, 0) });
        db.AddFix(new Fix { Identifier = "FIX2", PositionNm = new Vector2(5, 0) });
        db.AddFix(new Fix { Identifier = "FIX3", PositionNm = new Vector2(20, 0) });

        var nearby = db.GetFixesNear(new Vector2(0, 0), 10f);

        Assert.Equal(2, nearby.Count);
        Assert.Contains(nearby, f => f.Identifier == "FIX1");
        Assert.Contains(nearby, f => f.Identifier == "FIX2");
    }

    [Fact]
    public void AddProcedure_IncreasesProcedureCount()
    {
        var db = new NavigationDatabase();
        var proc = new Procedure
        {
            Identifier = "BGGLO2",
            Type = ProcedureType.SID,
            AirportIdentifier = "KSFO"
        };

        db.AddProcedure(proc);

        Assert.Equal(1, db.ProcedureCount);
    }

    [Fact]
    public void GetProcedure_ReturnsAddedProcedure()
    {
        var db = new NavigationDatabase();
        var proc = new Procedure
        {
            Identifier = "BGGLO2",
            Type = ProcedureType.SID,
            AirportIdentifier = "KSFO"
        };

        db.AddProcedure(proc);
        var retrieved = db.GetProcedure("KSFO", "BGGLO2");

        Assert.NotNull(retrieved);
        Assert.Equal("BGGLO2", retrieved.Identifier);
    }

    [Fact]
    public void GetSidsForRunway_ReturnsOnlySids()
    {
        var db = new NavigationDatabase();

        db.AddProcedure(new Procedure
        {
            Identifier = "BGGLO2",
            Type = ProcedureType.SID,
            AirportIdentifier = "KSFO",
            RunwayIdentifier = "28L"
        });

        db.AddProcedure(new Procedure
        {
            Identifier = "DYAMD3",
            Type = ProcedureType.STAR,
            AirportIdentifier = "KSFO"
        });

        var sids = db.GetSidsForRunway("KSFO", "28L");

        Assert.Single(sids);
        Assert.Equal(ProcedureType.SID, sids[0].Type);
    }

    [Fact]
    public void BuildDirectRoute_CreatesRouteWithTwoFixes()
    {
        var db = new NavigationDatabase();
        db.AddFix(new Fix { Identifier = "BEBOP", PositionNm = new Vector2(0, 0) });
        db.AddFix(new Fix { Identifier = "SUNST", PositionNm = new Vector2(10, 10) });

        var route = db.BuildDirectRoute("BEBOP", "SUNST");

        Assert.NotNull(route);
        Assert.Equal(2, route.Segments.Count);
        Assert.Equal("BEBOP", route.Segments[0].Fix.Identifier);
        Assert.Equal("SUNST", route.Segments[1].Fix.Identifier);
    }

    [Fact]
    public void BuildDirectRoute_InvalidFix_ReturnsNull()
    {
        var db = new NavigationDatabase();
        db.AddFix(new Fix { Identifier = "BEBOP", PositionNm = new Vector2(0, 0) });

        var route = db.BuildDirectRoute("BEBOP", "NOTHERE");

        Assert.Null(route);
    }

    [Fact]
    public void BuildRouteToFix_CreatesRouteFromCurrentPosition()
    {
        var db = new NavigationDatabase();
        db.AddFix(new Fix { Identifier = "BEBOP", PositionNm = new Vector2(10, 10) });

        var currentPosition = new Vector2(0, 0);
        var route = db.BuildRouteToFix(currentPosition, "BEBOP");

        Assert.NotNull(route);
        Assert.Equal(2, route.Segments.Count);
        Assert.Equal("BEBOP", route.Segments[1].Fix.Identifier);
    }
}
