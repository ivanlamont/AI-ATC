using System.Collections.Generic;

namespace AIATC.Domain.Models.Airspace;

/// <summary>
/// Sample airspace configuration for KSFO (San Francisco)
/// </summary>
public static class SampleAirspaceData
{
    /// <summary>
    /// Creates a realistic KSFO airspace configuration
    /// </summary>
    public static HandoffManager CreateKsfoAirspace()
    {
        var manager = new HandoffManager();

        // San Francisco Tower - Airport surface to 2,500 ft, 5 NM radius
        var tower = new Sector
        {
            Identifier = "SFO_TWR",
            Name = "San Francisco Tower",
            Type = SectorType.Tower,
            FrequencyMhz = 120.5f,
            SecondaryFrequencyMhz = 120.75f,
            ControllerCallsign = "San Francisco Tower",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 5.0f
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 0,
                MaximumAltitude = 2500
            },
            AdjacentSectors = new List<string> { "NCT_APP1", "NCT_DEP1" }
        };

        // NorCal Approach Sector 1 (West) - 2,500 to 10,000 ft
        var approach1 = new Sector
        {
            Identifier = "NCT_APP1",
            Name = "NorCal Approach West",
            Type = SectorType.Approach,
            FrequencyMhz = 135.1f,
            ControllerCallsign = "NorCal Approach",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(-50, -50),
                    new Vector2(-50, 50),
                    new Vector2(-5, 50),
                    new Vector2(-5, -50)
                }
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 2500,
                MaximumAltitude = 10000
            },
            AdjacentSectors = new List<string> { "SFO_TWR", "NCT_APP2", "OAK_CTR" }
        };

        // NorCal Approach Sector 2 (East) - 2,500 to 10,000 ft
        var approach2 = new Sector
        {
            Identifier = "NCT_APP2",
            Name = "NorCal Approach East",
            Type = SectorType.Approach,
            FrequencyMhz = 135.65f,
            ControllerCallsign = "NorCal Approach",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(5, -50),
                    new Vector2(5, 50),
                    new Vector2(50, 50),
                    new Vector2(50, -50)
                }
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 2500,
                MaximumAltitude = 10000
            },
            AdjacentSectors = new List<string> { "SFO_TWR", "NCT_APP1", "OAK_CTR" }
        };

        // NorCal Departure Sector 1 - 2,500 to 10,000 ft
        var departure1 = new Sector
        {
            Identifier = "NCT_DEP1",
            Name = "NorCal Departure",
            Type = SectorType.Departure,
            FrequencyMhz = 120.9f,
            ControllerCallsign = "NorCal Departure",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(-25, -25),
                    new Vector2(-25, 25),
                    new Vector2(25, 25),
                    new Vector2(25, -25)
                }
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 2500,
                MaximumAltitude = 10000
            },
            AdjacentSectors = new List<string> { "SFO_TWR", "OAK_CTR" }
        };

        // Oakland Center (High altitude) - Above 10,000 ft
        var center = new Sector
        {
            Identifier = "OAK_CTR",
            Name = "Oakland Center",
            Type = SectorType.Center,
            FrequencyMhz = 134.15f,
            ControllerCallsign = "Oakland Center",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(-100, -100),
                    new Vector2(-100, 100),
                    new Vector2(100, 100),
                    new Vector2(100, -100)
                }
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 10000,
                MaximumAltitude = null // Unlimited
            },
            AdjacentSectors = new List<string> { "NCT_APP1", "NCT_APP2", "NCT_DEP1" }
        };

        // Ground control (for completeness, not used in simulation)
        var ground = new Sector
        {
            Identifier = "SFO_GND",
            Name = "San Francisco Ground",
            Type = SectorType.Ground,
            FrequencyMhz = 121.8f,
            ControllerCallsign = "San Francisco Ground",
            Boundary = new SectorBoundary
            {
                Center = new Vector2(0, 0),
                RadiusNm = 2.0f
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 0,
                MaximumAltitude = 0 // Surface only
            },
            IsActive = false // Not used in air simulation
        };

        // Add all sectors
        manager.AddSector(tower);
        manager.AddSector(approach1);
        manager.AddSector(approach2);
        manager.AddSector(departure1);
        manager.AddSector(center);
        manager.AddSector(ground);

        return manager;
    }

    /// <summary>
    /// Creates a simple test airspace with 2 sectors
    /// </summary>
    public static HandoffManager CreateTestAirspace()
    {
        var manager = new HandoffManager();

        var sector1 = new Sector
        {
            Identifier = "TEST1",
            Name = "Test Sector 1",
            Type = SectorType.Approach,
            FrequencyMhz = 120.0f,
            ControllerCallsign = "Test Approach",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(-30, -30),
                    new Vector2(-30, 30),
                    new Vector2(0, 30),
                    new Vector2(0, -30)
                },
                Center = new Vector2(-15, 0)
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 0,
                MaximumAltitude = 10000
            },
            AdjacentSectors = new List<string> { "TEST2" }
        };

        var sector2 = new Sector
        {
            Identifier = "TEST2",
            Name = "Test Sector 2",
            Type = SectorType.Approach,
            FrequencyMhz = 121.0f,
            ControllerCallsign = "Test Approach",
            Boundary = new SectorBoundary
            {
                Vertices = new List<Vector2>
                {
                    new Vector2(0, -30),
                    new Vector2(0, 30),
                    new Vector2(30, 30),
                    new Vector2(30, -30)
                },
                Center = new Vector2(15, 0)
            },
            AltitudeLimits = new AltitudeLimit
            {
                MinimumAltitude = 0,
                MaximumAltitude = 10000
            },
            AdjacentSectors = new List<string> { "TEST1" }
        };

        manager.AddSector(sector1);
        manager.AddSector(sector2);

        return manager;
    }
}
