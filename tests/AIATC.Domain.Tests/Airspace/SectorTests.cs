using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Airspace;
using System.Collections.Generic;

namespace AIATC.Domain.Tests.Airspace;

public class SectorTests
{
    [Fact]
    public void CircularBoundary_ContainsPointInside()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 10
            }
        };

        var pointInside = new Vector2(5, 5);
        Assert.True(sector.ContainsPosition(pointInside));
    }

    [Fact]
    public void CircularBoundary_DoesNotContainPointOutside()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 10
            }
        };

        var pointOutside = new Vector2(15, 15);
        Assert.False(sector.ContainsPosition(pointOutside));
    }

    [Fact]
    public void PolygonBoundary_ContainsPointInside()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(0, 0),
                    new Vector2(10, 0),
                    new Vector2(10, 10),
                    new Vector2(0, 10)
                }
            }
        };

        var pointInside = new Vector2(5, 5);
        Assert.True(sector.ContainsPosition(pointInside));
    }

    [Fact]
    public void PolygonBoundary_DoesNotContainPointOutside()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(0, 0),
                    new Vector2(10, 0),
                    new Vector2(10, 10),
                    new Vector2(0, 10)
                }
            }
        };

        var pointOutside = new Vector2(15, 15);
        Assert.False(sector.ContainsPosition(pointOutside));
    }

    [Fact]
    public void AltitudeLimit_ContainsAltitudeInRange()
    {
        var limits = new AltitudeLimit
        {
            MinimumAltitude = 5000,
            MaximumAltitude = 10000
        };

        Assert.True(limits.ContainsAltitude(7000));
    }

    [Fact]
    public void AltitudeLimit_DoesNotContainAltitudeBelowMinimum()
    {
        var limits = new AltitudeLimit
        {
            MinimumAltitude = 5000,
            MaximumAltitude = 10000
        };

        Assert.False(limits.ContainsAltitude(4000));
    }

    [Fact]
    public void AltitudeLimit_DoesNotContainAltitudeAboveMaximum()
    {
        var limits = new AltitudeLimit
        {
            MinimumAltitude = 5000,
            MaximumAltitude = 10000
        };

        Assert.False(limits.ContainsAltitude(11000));
    }

    [Fact]
    public void AltitudeLimit_UnlimitedMaximum_ContainsHighAltitude()
    {
        var limits = new AltitudeLimit
        {
            MinimumAltitude = 5000,
            MaximumAltitude = null // Unlimited
        };

        Assert.True(limits.ContainsAltitude(50000));
    }

    [Fact]
    public void ContainsAircraft_ChecksBothLateralAndVertical()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 10
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 5000,
                MaximumAltitude = 10000
            }
        };

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(5, 5),
            AltitudeFt = 7000
        };

        Assert.True(sector.ContainsAircraft(aircraft));
    }

    [Fact]
    public void ContainsAircraft_ReturnsFalseWhenOutsideLateral()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 10
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 5000,
                MaximumAltitude = 10000
            }
        };

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(20, 20), // Outside
            AltitudeFt = 7000
        };

        Assert.False(sector.ContainsAircraft(aircraft));
    }

    [Fact]
    public void ContainsAircraft_ReturnsFalseWhenOutsideVertical()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 10
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 5000,
                MaximumAltitude = 10000
            }
        };

        var aircraft = new AircraftModel
        {
            PositionNm = new Vector2(5, 5),
            AltitudeFt = 12000 // Too high
        };

        Assert.False(sector.ContainsAircraft(aircraft));
    }

    [Fact]
    public void GetDistanceToBoundary_CircularSector()
    {
        var sector = new Sector
        {
            Identifier = "TEST",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 10
            }
        };

        // Point at center
        var distance = sector.GetDistanceToBoundary(new Vector2(0, 0));
        Assert.Equal(10f, distance, 1);

        // Point on edge
        distance = sector.GetDistanceToBoundary(new Vector2(10, 0));
        Assert.Equal(0f, distance, 1);
    }
}
