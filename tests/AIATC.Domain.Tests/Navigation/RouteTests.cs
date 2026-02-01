using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Navigation;

namespace AIATC.Domain.Tests.Navigation;

public class RouteTests
{
    [Fact]
    public void AddFix_FirstFix_HasZeroDistance()
    {
        var route = new Route();
        var fix = new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(10, 10)
        };

        route.AddFix(fix);

        Assert.Single(route.Segments);
        Assert.Equal(0f, route.Segments[0].DistanceNm);
    }

    [Fact]
    public void AddFix_SecondFix_CalculatesDistance()
    {
        var route = new Route();
        var fix1 = new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(0, 0)
        };
        var fix2 = new Fix
        {
            Identifier = "SUNST",
            PositionNm = new Vector2(3, 4)
        };

        route.AddFix(fix1);
        route.AddFix(fix2);

        Assert.Equal(2, route.Segments.Count);
        Assert.Equal(5.0f, route.Segments[1].DistanceNm, 2);
    }

    [Fact]
    public void AddFix_SecondFix_CalculatesCourse()
    {
        var route = new Route();
        var fix1 = new Fix
        {
            Identifier = "BEBOP",
            PositionNm = new Vector2(0, 0)
        };
        var fix2 = new Fix
        {
            Identifier = "SUNST",
            PositionNm = new Vector2(0, 10) // Due north
        };

        route.AddFix(fix1);
        route.AddFix(fix2);

        Assert.Equal(0f, route.Segments[1].CourseDegrees, 1);
    }

    [Fact]
    public void TotalDistanceNm_SumsAllSegments()
    {
        var route = new Route();
        route.AddFix(new Fix { Identifier = "FIX1", PositionNm = new Vector2(0, 0) });
        route.AddFix(new Fix { Identifier = "FIX2", PositionNm = new Vector2(3, 4) });  // 5nm
        route.AddFix(new Fix { Identifier = "FIX3", PositionNm = new Vector2(6, 8) });  // 5nm more

        Assert.Equal(10.0f, route.TotalDistanceNm, 2);
    }

    [Fact]
    public void GetNextFix_ReturnsFixAheadOnRoute()
    {
        var route = new Route();
        var fix1 = new Fix { Identifier = "FIX1", PositionNm = new Vector2(0, 0) };
        var fix2 = new Fix { Identifier = "FIX2", PositionNm = new Vector2(10, 0) };
        var fix3 = new Fix { Identifier = "FIX3", PositionNm = new Vector2(20, 0) };

        route.AddFix(fix1);
        route.AddFix(fix2);
        route.AddFix(fix3);

        var currentPosition = new Vector2(5, 0); // Between FIX1 and FIX2
        var nextFix = route.GetNextFix(currentPosition);

        Assert.NotNull(nextFix);
        Assert.Equal("FIX2", nextFix.Identifier);
    }

    [Fact]
    public void GetNextFix_AtLastFix_ReturnsNull()
    {
        var route = new Route();
        route.AddFix(new Fix { Identifier = "FIX1", PositionNm = new Vector2(0, 0) });
        route.AddFix(new Fix { Identifier = "FIX2", PositionNm = new Vector2(10, 0) });

        var currentPosition = new Vector2(10, 0); // At last fix
        var nextFix = route.GetNextFix(currentPosition);

        Assert.Null(nextFix);
    }

    [Fact]
    public void GetCourseToNextFix_ReturnsCorrectCourse()
    {
        var route = new Route();
        route.AddFix(new Fix { Identifier = "FIX1", PositionNm = new Vector2(0, 0) });
        route.AddFix(new Fix { Identifier = "FIX2", PositionNm = new Vector2(0, 10) }); // North

        var currentPosition = new Vector2(0, 5);
        var course = route.GetCourseToNextFix(currentPosition);

        Assert.NotNull(course);
        Assert.Equal(0f, course.Value, 1); // Should be heading north
    }
}
