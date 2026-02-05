using AIATC.Domain.Models.Scenarios;
using AIATC.Domain.Services;
using System;
using System.Linq;
using Xunit;

namespace AIATC.Domain.Tests.Scenarios;

public class ScenarioServiceTests
{
    [Fact]
    public void RegisterScenario_AddsToCollection()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();

        service.RegisterScenario(scenario);

        var retrieved = service.GetScenario(scenario.Metadata.Id);
        Assert.NotNull(retrieved);
        Assert.Equal(scenario.Metadata.Id, retrieved.Metadata.Id);
    }

    [Fact]
    public void GetAllScenarios_ReturnsAllRegistered()
    {
        var service = new ScenarioService();
        service.RegisterScenario(ScenarioTemplates.CreateBeginnerTraining());
        service.RegisterScenario(ScenarioTemplates.CreateRushHour());

        var scenarios = service.GetAllScenarios().ToList();

        Assert.Equal(2, scenarios.Count);
    }

    [Fact]
    public void GetScenariosByDifficulty_FiltersCorrectly()
    {
        var service = new ScenarioService();
        service.RegisterScenario(ScenarioTemplates.CreateBeginnerTraining()); // Easy
        service.RegisterScenario(ScenarioTemplates.CreateRushHour());         // Medium
        service.RegisterScenario(ScenarioTemplates.CreateStormChallenge());   // Hard

        var easyScenarios = service.GetScenariosByDifficulty(ScenarioDifficulty.Easy).ToList();
        var mediumScenarios = service.GetScenariosByDifficulty(ScenarioDifficulty.Medium).ToList();

        Assert.Single(easyScenarios);
        Assert.Single(mediumScenarios);
    }

    [Fact]
    public void GetScenariosByTags_FiltersCorrectly()
    {
        var service = new ScenarioService();
        service.RegisterScenario(ScenarioTemplates.CreateBeginnerTraining()); // "training" tag
        service.RegisterScenario(ScenarioTemplates.CreateRushHour());         // "busy" tag

        var trainingScenarios = service.GetScenariosByTags("training").ToList();

        Assert.Single(trainingScenarios);
        Assert.Contains(trainingScenarios, s => s.Metadata.Tags.Contains("training"));
    }

    [Fact]
    public void StartScenario_SetsActiveScenario()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);

        service.StartScenario(scenario.Metadata.Id);

        var active = service.GetActiveScenario();
        Assert.NotNull(active);
        Assert.Equal(ScenarioState.Running, active.State);
    }

    [Fact]
    public void StartScenario_NotFound_ThrowsException()
    {
        var service = new ScenarioService();

        Assert.Throws<ArgumentException>(() => service.StartScenario("invalid-id"));
    }

    [Fact]
    public void StartScenario_AnotherRunning_ThrowsException()
    {
        var service = new ScenarioService();
        service.RegisterScenario(ScenarioTemplates.CreateBeginnerTraining());
        service.RegisterScenario(ScenarioTemplates.CreateRushHour());

        service.StartScenario("beginner-training");

        Assert.Throws<InvalidOperationException>(() => service.StartScenario("rush-hour"));
    }

    [Fact]
    public void StartScenario_RaisesEvent()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);

        var eventRaised = false;
        service.ScenarioStateChanged += (sender, args) =>
        {
            eventRaised = true;
            Assert.Equal(ScenarioState.Running, args.NewState);
            Assert.Equal(ScenarioState.NotStarted, args.PreviousState);
        };

        service.StartScenario(scenario.Metadata.Id);

        Assert.True(eventRaised);
    }

    [Fact]
    public void UpdateActiveScenario_NoActive_DoesNothing()
    {
        var service = new ScenarioService();

        // Should not throw
        service.UpdateActiveScenario(1.0f);
    }

    [Fact]
    public void UpdateActiveScenario_UpdatesTime()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        service.UpdateActiveScenario(1.5f);

        var active = service.GetActiveScenario();
        Assert.Equal(1.5f, active!.ElapsedTimeSeconds);
    }

    [Fact]
    public void PauseActiveScenario_PausesScenario()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        service.PauseActiveScenario();

        var active = service.GetActiveScenario();
        Assert.Equal(ScenarioState.Paused, active!.State);
    }

    [Fact]
    public void ResumeActiveScenario_ResumesScenario()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);
        service.PauseActiveScenario();

        service.ResumeActiveScenario();

        var active = service.GetActiveScenario();
        Assert.Equal(ScenarioState.Running, active!.State);
    }

    [Fact]
    public void CompleteActiveScenario_CompletesScenario()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        service.CompleteActiveScenario();

        var active = service.GetActiveScenario();
        Assert.Equal(ScenarioState.Completed, active!.State);
        Assert.NotNull(active.Result);
        Assert.True(active.Result.Success);
    }

    [Fact]
    public void FailActiveScenario_FailsScenario()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        service.FailActiveScenario("Test failure");

        var active = service.GetActiveScenario();
        Assert.Equal(ScenarioState.Failed, active!.State);
        Assert.NotNull(active.Result);
        Assert.False(active.Result.Success);
    }

    [Fact]
    public void RecordAircraftLanding_IncrementsCount()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        service.RecordAircraftLanding();
        service.RecordAircraftLanding();

        var active = service.GetActiveScenario();
        Assert.Equal(2, active!.AircraftLanded);
    }

    [Fact]
    public void RecordAircraftLanding_UpdatesObjective()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        var landObjective = scenario.Objectives
            .First(o => o.Type == ObjectiveType.LandAircraft);

        service.RecordAircraftLanding();

        Assert.Equal(1f, landObjective.CurrentValue);
    }

    [Fact]
    public void RecordSeparationViolation_IncrementsCount()
    {
        var service = new ScenarioService();
        var scenario = ScenarioTemplates.CreateBeginnerTraining();
        service.RegisterScenario(scenario);
        service.StartScenario(scenario.Metadata.Id);

        service.RecordSeparationViolation();

        var active = service.GetActiveScenario();
        Assert.Equal(1, active!.SeparationViolations);
    }

    [Fact]
    public void UpdateObjectiveProgress_UpdatesValue()
    {
        var service = new ScenarioService();
        var scenario = new Scenario
        {
            Metadata = new ScenarioMetadata { Id = "test" },
            Objectives = new System.Collections.Generic.List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Id = "obj-1",
                    TargetValue = 10
                }
            }
        };
        service.RegisterScenario(scenario);
        service.StartScenario("test");

        service.UpdateObjectiveProgress("obj-1", 5);

        Assert.Equal(5f, scenario.Objectives[0].CurrentValue);
    }

    [Fact]
    public void UpdateObjectiveProgress_CompletesObjective_RaisesEvent()
    {
        var service = new ScenarioService();
        var scenario = new Scenario
        {
            Metadata = new ScenarioMetadata { Id = "test" },
            Objectives = new System.Collections.Generic.List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Id = "obj-1",
                    TargetValue = 10
                }
            }
        };
        service.RegisterScenario(scenario);
        service.StartScenario("test");

        var eventRaised = false;
        service.ObjectiveCompleted += (sender, args) =>
        {
            eventRaised = true;
            Assert.Equal("obj-1", args.Objective.Id);
        };

        service.UpdateObjectiveProgress("obj-1", 10);

        Assert.True(eventRaised);
    }

    [Fact]
    public void Clear_RemovesAllScenarios()
    {
        var service = new ScenarioService();
        service.RegisterScenario(ScenarioTemplates.CreateBeginnerTraining());
        service.RegisterScenario(ScenarioTemplates.CreateRushHour());

        service.Clear();

        Assert.Empty(service.GetAllScenarios());
        Assert.Null(service.GetActiveScenario());
    }
}
