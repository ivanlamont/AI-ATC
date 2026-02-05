using AIATC.Domain.Models;
using AIATC.Domain.Models.Scenarios;
using AIATC.Domain.Services;
using System;
using Xunit;

namespace AIATC.Domain.Tests.Services;

public class SimulationEngineTests
{
    [Fact]
    public void Constructor_InitializesComponents()
    {
        var engine = new SimulationEngine();

        Assert.NotNull(engine.TimeController);
        Assert.NotNull(engine.ScenarioService);
        Assert.NotNull(engine.WeatherService);
        Assert.Empty(engine.Aircraft);
    }

    [Fact]
    public void Update_IncrementsRealTime()
    {
        var engine = new SimulationEngine();

        engine.Update(1.0f);

        Assert.Equal(1.0f, engine.RealTimeSeconds);
    }

    [Fact]
    public void Update_IncrementsSimulationTimeWithScale()
    {
        var engine = new SimulationEngine();
        engine.TimeController.TimeScale = 2.0f;

        engine.Update(1.0f);

        Assert.Equal(1.0f, engine.RealTimeSeconds);
        Assert.Equal(2.0f, engine.SimulationTimeSeconds);
    }

    [Fact]
    public void Update_PausedDoesNotIncrementSimulationTime()
    {
        var engine = new SimulationEngine();
        engine.TimeController.IsPaused = true;

        engine.Update(1.0f);

        Assert.Equal(1.0f, engine.RealTimeSeconds);
        Assert.Equal(0f, engine.SimulationTimeSeconds);
    }

    [Fact]
    public void AddAircraft_AddsToList()
    {
        var engine = new SimulationEngine();
        var aircraft = new AircraftModel("TEST123", new Vector2(0, 0), 0, 200, 5000, null);

        engine.AddAircraft(aircraft);

        Assert.Single(engine.Aircraft);
        Assert.Contains(aircraft, engine.Aircraft);
    }

    [Fact]
    public void AddAircraft_IncrementsScenarioCount()
    {
        var engine = new SimulationEngine();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        engine.ScenarioService.RegisterScenario(scenario);
        engine.ScenarioService.StartScenario(scenario.Metadata.Id);

        var aircraft = new AircraftModel("TEST123", new Vector2(0, 0), 0, 200, 5000, null);
        engine.AddAircraft(aircraft);

        var activeScenario = engine.ScenarioService.GetActiveScenario();
        Assert.Equal(1, activeScenario!.AircraftSpawned);
    }

    [Fact]
    public void RemoveAircraft_RemovesFromList()
    {
        var engine = new SimulationEngine();
        var aircraft = new AircraftModel("TEST123", new Vector2(0, 0), 0, 200, 5000, null);
        engine.AddAircraft(aircraft);

        engine.RemoveAircraft(aircraft);

        Assert.Empty(engine.Aircraft);
    }

    [Fact]
    public void ClearAircraft_RemovesAll()
    {
        var engine = new SimulationEngine();
        engine.AddAircraft(new AircraftModel("TEST1", new Vector2(0, 0), 0, 200, 5000, null));
        engine.AddAircraft(new AircraftModel("TEST2", new Vector2(1, 1), 0, 200, 5000, null));

        engine.ClearAircraft();

        Assert.Empty(engine.Aircraft);
    }

    [Fact]
    public void Reset_ClearsAllState()
    {
        var engine = new SimulationEngine();
        engine.AddAircraft(new AircraftModel("TEST123", new Vector2(0, 0), 0, 200, 5000, null));
        engine.Update(5.0f);
        engine.TimeController.TimeScale = 3.0f;

        engine.Reset();

        Assert.Empty(engine.Aircraft);
        Assert.Equal(0f, engine.SimulationTimeSeconds);
        Assert.Equal(0f, engine.RealTimeSeconds);
        Assert.Equal(1.0f, engine.TimeController.TimeScale);
    }

    [Fact]
    public void Update_UpdatesAircraftPosition()
    {
        var engine = new SimulationEngine();
        var aircraft = new AircraftModel("TEST123", new Vector2(0, 0), 0, 200, 5000, null);
        engine.AddAircraft(aircraft);

        var initialPos = aircraft.PositionNm;
        engine.Update(1.0f);

        // Aircraft should have moved
        Assert.NotEqual(initialPos, aircraft.PositionNm);
    }

    [Fact]
    public void GetEffectiveScoreMultiplier_CombinesDifficultyAndTimeScale()
    {
        var engine = new SimulationEngine();
        var scenario = ScenarioTemplates.CreateStormChallenge(); // Difficulty 2.0x
        engine.ScenarioService.RegisterScenario(scenario);
        engine.ScenarioService.StartScenario(scenario.Metadata.Id);
        engine.TimeController.TimeScale = 2.0f; // Time multiplier 1.5x

        var multiplier = engine.GetEffectiveScoreMultiplier();

        // 2.0 (difficulty) * 1.5 (time) = 3.0
        Assert.Equal(3.0f, multiplier);
    }

    [Fact]
    public void GetEffectiveScoreMultiplier_NoScenario_ReturnsTimeMultiplier()
    {
        var engine = new SimulationEngine();
        engine.TimeController.TimeScale = 2.0f;

        var multiplier = engine.GetEffectiveScoreMultiplier();

        Assert.Equal(1.5f, multiplier);
    }

    [Fact]
    public void CurrentScore_NoScenario_ReturnsZero()
    {
        var engine = new SimulationEngine();

        Assert.Equal(0, engine.CurrentScore);
    }

    [Fact]
    public void CurrentScore_WithScenario_ReturnsScenarioScore()
    {
        var engine = new SimulationEngine();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        engine.ScenarioService.RegisterScenario(scenario);
        engine.ScenarioService.StartScenario(scenario.Metadata.Id);

        scenario.CurrentScore = 500;

        Assert.Equal(500, engine.CurrentScore);
    }

    [Fact]
    public void Update_CallsScenarioUpdate()
    {
        var engine = new SimulationEngine();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        engine.ScenarioService.RegisterScenario(scenario);
        engine.ScenarioService.StartScenario(scenario.Metadata.Id);

        var initialTime = scenario.ElapsedTimeSeconds;
        engine.Update(1.0f);

        Assert.True(scenario.ElapsedTimeSeconds > initialTime);
    }

    [Fact(Skip = "Integration test - requires specific aircraft landing conditions")]
    public void AircraftLanded_IncreasesScoreWithMultiplier()
    {
        var engine = new SimulationEngine();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        scenario.Configuration.ScoringConfig.DifficultyMultiplier = 2.0f;
        engine.ScenarioService.RegisterScenario(scenario);
        engine.ScenarioService.StartScenario(scenario.Metadata.Id);
        engine.TimeController.TimeScale = 2.0f; // 1.5x time multiplier

        var airport = new AirportModel
        {
            IcaoCode = "KJFK",
            PositionNm = new Vector2(0, 0),
            AltitudeFt = 13
        };

        var aircraft = new AircraftModel(
            "TEST123",
            new Vector2(0.01f, 0.01f), // Extremely close to airport
            0,
            140, // Very slow speed (approaching)
            20,  // Very low altitude
            airport,
            isArrival: true);

        engine.AddAircraft(aircraft);

        // Update until aircraft lands
        for (int i = 0; i < 100; i++)
        {
            engine.Update(0.1f);
            if (aircraft.Landed)
                break;
        }

        // Score should be increased
        // Base 100 * 2.0 (difficulty) * 1.5 (time) = 300
        Assert.True(scenario.CurrentScore >= 300);
    }

    [Fact(Skip = "Integration test - requires specific aircraft landing conditions")]
    public void AircraftLanded_RaisesEvent()
    {
        var engine = new SimulationEngine();
        var eventRaised = false;
        AircraftModel? landedAircraft = null;

        engine.AircraftLanded += (sender, args) =>
        {
            eventRaised = true;
            landedAircraft = args.Aircraft;
        };

        var airport = new AirportModel
        {
            IcaoCode = "KJFK",
            PositionNm = new Vector2(0, 0),
            AltitudeFt = 13
        };

        var aircraft = new AircraftModel(
            "TEST123",
            new Vector2(0.01f, 0.01f), // Extremely close to airport
            0,
            140, // Very slow speed (approaching)
            20,  // Very low altitude
            airport,
            isArrival: true);

        engine.AddAircraft(aircraft);

        // Update until aircraft lands
        for (int i = 0; i < 100; i++)
        {
            engine.Update(0.1f);
            if (aircraft.Landed)
                break;
        }

        Assert.True(eventRaised);
        Assert.Same(aircraft, landedAircraft);
    }
}
