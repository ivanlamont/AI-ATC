using Xunit;
using AIATC.Domain.Models;
using AIATC.Domain.Models.Airspace;
using System.Collections.Generic;

namespace AIATC.Domain.Tests.Airspace;

public class HandoffManagerTests
{
    [Fact]
    public void AddSector_IncreasesCount()
    {
        var manager = new HandoffManager();
        var sector = new Sector { Identifier = "TEST1" };

        manager.AddSector(sector);

        Assert.Single(manager.GetAllSectors());
    }

    [Fact]
    public void GetSector_ReturnsAddedSector()
    {
        var manager = new HandoffManager();
        var sector = new Sector
        {
            Identifier = "TEST1",
            Name = "Test Sector"
        };

        manager.AddSector(sector);
        var retrieved = manager.GetSector("TEST1");

        Assert.NotNull(retrieved);
        Assert.Equal("Test Sector", retrieved.Name);
    }

    [Fact]
    public void AssignAircraftToSector_TracksAssignment()
    {
        var manager = new HandoffManager();
        var sector = new Sector { Identifier = "TEST1" };
        manager.AddSector(sector);

        manager.AssignAircraftToSector("UAL123", "TEST1");

        var assignedSector = manager.GetAircraftSector("UAL123");
        Assert.NotNull(assignedSector);
        Assert.Equal("TEST1", assignedSector.Identifier);
    }

    [Fact]
    public void AutoAssignSector_AssignsBasedOnPosition()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();

        var aircraft = new AircraftModel
        {
            Callsign = "UAL123",
            PositionNm = new Vector2(-10, 0), // In TEST1
            AltitudeFt = 5000
        };

        manager.AutoAssignSector(aircraft);

        var sector = manager.GetAircraftSector("UAL123");
        Assert.NotNull(sector);
        Assert.Equal("TEST1", sector.Identifier);
    }

    [Fact]
    public void CheckHandoffNeeded_DetectsWhenNearBoundary()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();

        var aircraft = new AircraftModel
        {
            Callsign = "UAL123",
            PositionNm = new Vector2(-2, 0), // Near boundary at x=0
            AltitudeFt = 5000,
            HeadingDegrees = 90 // Heading east (towards TEST2)
        };

        manager.AssignAircraftToSector("UAL123", "TEST1");

        var recommendation = manager.CheckHandoffNeeded(aircraft);

        Assert.NotNull(recommendation);
        Assert.Equal("TEST1", recommendation.CurrentSector.Identifier);
        Assert.Equal("TEST2", recommendation.TargetSector.Identifier);
    }

    [Fact]
    public void CheckHandoffNeeded_ReturnsNullWhenSafe()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();

        var aircraft = new AircraftModel
        {
            Callsign = "UAL123",
            PositionNm = new Vector2(-20, 0), // Far from boundary
            AltitudeFt = 5000
        };

        manager.AssignAircraftToSector("UAL123", "TEST1");

        var recommendation = manager.CheckHandoffNeeded(aircraft);

        Assert.Null(recommendation);
    }

    [Fact]
    public void InitiateHandoff_CreatesPendingHandoff()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();
        manager.AssignAircraftToSector("UAL123", "TEST1");

        manager.InitiateHandoff("UAL123", "TEST2");

        var pending = manager.GetPendingHandoff("UAL123");
        Assert.NotNull(pending);
        Assert.Equal("TEST1", pending.FromSector.Identifier);
        Assert.Equal("TEST2", pending.ToSector.Identifier);
        Assert.Equal(HandoffStatus.Initiated, pending.Status);
    }

    [Fact]
    public void AcceptHandoff_CompletesHandoff()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();
        manager.AssignAircraftToSector("UAL123", "TEST1");
        manager.InitiateHandoff("UAL123", "TEST2");

        var accepted = manager.AcceptHandoff("UAL123");

        Assert.True(accepted);

        // Should be reassigned to TEST2
        var sector = manager.GetAircraftSector("UAL123");
        Assert.Equal("TEST2", sector?.Identifier);

        // Pending handoff should be removed
        var pending = manager.GetPendingHandoff("UAL123");
        Assert.Null(pending);
    }

    [Fact]
    public void CheckHandoffNeeded_ImmedidateWhenCrossedBoundary()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();

        var aircraft = new AircraftModel
        {
            Callsign = "UAL123",
            PositionNm = new Vector2(5, 0), // In TEST2
            AltitudeFt = 5000
        };

        // But still assigned to TEST1
        manager.AssignAircraftToSector("UAL123", "TEST1");

        var recommendation = manager.CheckHandoffNeeded(aircraft);

        Assert.NotNull(recommendation);
        Assert.Equal(HandoffUrgency.Immediate, recommendation.Urgency);
    }

    [Fact]
    public void GetAllPendingHandoffs_ReturnsAllHandoffs()
    {
        var manager = SampleAirspaceData.CreateTestAirspace();

        manager.AssignAircraftToSector("UAL123", "TEST1");
        manager.AssignAircraftToSector("DAL456", "TEST1");

        manager.InitiateHandoff("UAL123", "TEST2");
        manager.InitiateHandoff("DAL456", "TEST2");

        var pending = manager.GetAllPendingHandoffs();
        Assert.Equal(2, pending.Count);
    }

    [Fact]
    public void SampleAirspace_CreatesKsfoAirspace()
    {
        var manager = SampleAirspaceData.CreateKsfoAirspace();

        var sectors = manager.GetAllSectors();
        Assert.True(sectors.Count >= 5);

        var tower = manager.GetSector("SFO_TWR");
        Assert.NotNull(tower);
        Assert.Equal(SectorType.Tower, tower.Type);
        Assert.Equal(120.5f, tower.FrequencyMhz);
    }
}
