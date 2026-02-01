using AIATC.Domain.Models.Scenarios;
using Xunit;

namespace AIATC.Domain.Tests.Scenarios;

public class ScenarioObjectiveTests
{
    [Fact]
    public void GetCompletionPercentage_NoProgress_ReturnsZero()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 0
        };

        Assert.Equal(0f, objective.GetCompletionPercentage());
    }

    [Fact]
    public void GetCompletionPercentage_HalfProgress_ReturnsFifty()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 5
        };

        Assert.Equal(50f, objective.GetCompletionPercentage());
    }

    [Fact]
    public void GetCompletionPercentage_Complete_ReturnsHundred()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 10,
            IsCompleted = true
        };

        Assert.Equal(100f, objective.GetCompletionPercentage());
    }

    [Fact]
    public void GetCompletionPercentage_OverTarget_ClampsToHundred()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 15
        };

        Assert.Equal(100f, objective.GetCompletionPercentage());
    }

    [Fact]
    public void UpdateProgress_ReachesTarget_MarksComplete()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 5
        };

        objective.UpdateProgress(10);

        Assert.Equal(10f, objective.CurrentValue);
        Assert.True(objective.IsCompleted);
    }

    [Fact]
    public void UpdateProgress_ExceedsTarget_MarksComplete()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 0
        };

        objective.UpdateProgress(15);

        Assert.Equal(15f, objective.CurrentValue);
        Assert.True(objective.IsCompleted);
    }

    [Fact]
    public void UpdateProgress_BelowTarget_RemainsIncomplete()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 0
        };

        objective.UpdateProgress(5);

        Assert.Equal(5f, objective.CurrentValue);
        Assert.False(objective.IsCompleted);
    }

    [Fact]
    public void UpdateProgress_AlreadyCompleted_RemainsComplete()
    {
        var objective = new ScenarioObjective
        {
            TargetValue = 10,
            CurrentValue = 10,
            IsCompleted = true
        };

        objective.UpdateProgress(8); // Going backwards

        Assert.Equal(8f, objective.CurrentValue);
        Assert.True(objective.IsCompleted); // Stays completed
    }
}
