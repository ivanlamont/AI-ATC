using AIATC.Domain.Models.Scenarios;
using System;
using System.Linq;
using Xunit;

namespace AIATC.Domain.Tests.Scenarios;

public class ScenarioTests
{
    [Fact]
    public void Start_NotStartedScenario_SetsRunningState()
    {
        var scenario = new Scenario();

        scenario.Start();

        Assert.Equal(ScenarioState.Running, scenario.State);
        Assert.NotNull(scenario.StartTime);
        Assert.Equal(0f, scenario.ElapsedTimeSeconds);
    }

    [Fact]
    public void Start_AlreadyStarted_ThrowsException()
    {
        var scenario = new Scenario();
        scenario.Start();

        Assert.Throws<InvalidOperationException>(() => scenario.Start());
    }

    [Fact]
    public void Update_RunningScenario_IncrementsTime()
    {
        var scenario = new Scenario();
        scenario.Start();

        scenario.Update(1.0f);

        Assert.Equal(1.0f, scenario.ElapsedTimeSeconds);

        scenario.Update(2.5f);

        Assert.Equal(3.5f, scenario.ElapsedTimeSeconds);
    }

    [Fact]
    public void Update_PausedScenario_DoesNotIncrementTime()
    {
        var scenario = new Scenario();
        scenario.Start();
        scenario.Update(1.0f);
        scenario.Pause();

        scenario.Update(5.0f);

        Assert.Equal(1.0f, scenario.ElapsedTimeSeconds);
    }

    [Fact]
    public void Pause_RunningScenario_SetsPausedState()
    {
        var scenario = new Scenario();
        scenario.Start();

        scenario.Pause();

        Assert.Equal(ScenarioState.Paused, scenario.State);
    }

    [Fact]
    public void Resume_PausedScenario_SetsRunningState()
    {
        var scenario = new Scenario();
        scenario.Start();
        scenario.Pause();

        scenario.Resume();

        Assert.Equal(ScenarioState.Running, scenario.State);
    }

    [Fact]
    public void Complete_RunningScenario_SetsCompletedState()
    {
        var scenario = new Scenario();
        scenario.Start();

        scenario.Complete();

        Assert.Equal(ScenarioState.Completed, scenario.State);
        Assert.NotNull(scenario.EndTime);
        Assert.NotNull(scenario.Result);
        Assert.True(scenario.Result.Success);
    }

    [Fact]
    public void Fail_RunningScenario_SetsFailedState()
    {
        var scenario = new Scenario();
        scenario.Start();

        scenario.Fail("Test failure");

        Assert.Equal(ScenarioState.Failed, scenario.State);
        Assert.NotNull(scenario.EndTime);
        Assert.NotNull(scenario.Result);
        Assert.False(scenario.Result.Success);
        Assert.Equal("Test failure", scenario.Result.FailureReason);
    }

    [Fact]
    public void AreRequiredObjectivesComplete_AllComplete_ReturnsTrue()
    {
        var scenario = new Scenario
        {
            Objectives = new System.Collections.Generic.List<ScenarioObjective>
            {
                new ScenarioObjective { IsRequired = true, IsCompleted = true },
                new ScenarioObjective { IsRequired = true, IsCompleted = true },
                new ScenarioObjective { IsRequired = false, IsCompleted = false }
            }
        };

        Assert.True(scenario.AreRequiredObjectivesComplete());
    }

    [Fact]
    public void AreRequiredObjectivesComplete_SomeIncomplete_ReturnsFalse()
    {
        var scenario = new Scenario
        {
            Objectives = new System.Collections.Generic.List<ScenarioObjective>
            {
                new ScenarioObjective { IsRequired = true, IsCompleted = true },
                new ScenarioObjective { IsRequired = true, IsCompleted = false },
                new ScenarioObjective { IsRequired = false, IsCompleted = false }
            }
        };

        Assert.False(scenario.AreRequiredObjectivesComplete());
    }

    [Fact]
    public void GetCompletionPercentage_HalfComplete_ReturnsFifty()
    {
        var scenario = new Scenario
        {
            Objectives = new System.Collections.Generic.List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    IsRequired = true,
                    TargetValue = 10,
                    CurrentValue = 5
                },
                new ScenarioObjective
                {
                    IsRequired = true,
                    TargetValue = 20,
                    CurrentValue = 10
                }
            }
        };

        Assert.Equal(50f, scenario.GetCompletionPercentage());
    }

    [Fact]
    public void GetDuration_NotStarted_ReturnsNull()
    {
        var scenario = new Scenario();

        Assert.Null(scenario.GetDuration());
    }

    [Fact]
    public void GetDuration_Running_ReturnsTimeSpan()
    {
        var scenario = new Scenario();
        scenario.Start();

        System.Threading.Thread.Sleep(100); // Wait a bit

        var duration = scenario.GetDuration();

        Assert.NotNull(duration);
        Assert.True(duration.Value.TotalMilliseconds >= 100);
    }

    [Fact]
    public void Update_TimeLimitExceeded_FailsScenario()
    {
        var scenario = new Scenario
        {
            Objectives = new System.Collections.Generic.List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Type = ObjectiveType.TimeLimit,
                    TargetValue = 10f, // 10 seconds
                    IsRequired = true
                },
                new ScenarioObjective
                {
                    Type = ObjectiveType.LandAircraft,
                    TargetValue = 5,
                    IsRequired = true
                }
            }
        };

        scenario.Start();
        scenario.Update(11f); // Exceed time limit

        Assert.Equal(ScenarioState.Failed, scenario.State);
        Assert.NotNull(scenario.Result);
        Assert.False(scenario.Result.Success);
    }
}
