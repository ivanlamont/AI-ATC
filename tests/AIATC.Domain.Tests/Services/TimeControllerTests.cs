using AIATC.Domain.Services;
using System;
using Xunit;

namespace AIATC.Domain.Tests.Services;

public class TimeControllerTests
{
    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        var controller = new TimeController();

        Assert.Equal(1.0f, controller.TimeScale);
        Assert.False(controller.IsPaused);
        Assert.Equal(0.1f, controller.MinTimeScale);
        Assert.Equal(5.0f, controller.MaxTimeScale);
    }

    [Fact]
    public void TimeScale_ClampsToMinimum()
    {
        var controller = new TimeController();

        controller.TimeScale = 0.05f;

        Assert.Equal(0.1f, controller.TimeScale);
    }

    [Fact]
    public void TimeScale_ClampsToMaximum()
    {
        var controller = new TimeController();

        controller.TimeScale = 10.0f;

        Assert.Equal(5.0f, controller.TimeScale);
    }

    [Fact]
    public void TimeScale_RaisesEventOnChange()
    {
        var controller = new TimeController();
        var eventRaised = false;
        float oldValue = 0;
        float newValue = 0;

        controller.TimeScaleChanged += (sender, args) =>
        {
            eventRaised = true;
            oldValue = args.OldTimeScale;
            newValue = args.NewTimeScale;
        };

        controller.TimeScale = 2.0f;

        Assert.True(eventRaised);
        Assert.Equal(1.0f, oldValue);
        Assert.Equal(2.0f, newValue);
    }

    [Fact]
    public void IsPaused_RaisesEventOnChange()
    {
        var controller = new TimeController();
        var eventRaised = false;
        bool isPaused = false;

        controller.PauseStateChanged += (sender, args) =>
        {
            eventRaised = true;
            isPaused = args.IsPaused;
        };

        controller.IsPaused = true;

        Assert.True(eventRaised);
        Assert.True(isPaused);
    }

    [Fact]
    public void EffectiveTimeScale_ReturnsZeroWhenPaused()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f,
            IsPaused = true
        };

        Assert.Equal(0f, controller.EffectiveTimeScale);
    }

    [Fact]
    public void EffectiveTimeScale_ReturnsTimeScaleWhenNotPaused()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f,
            IsPaused = false
        };

        Assert.Equal(2.0f, controller.EffectiveTimeScale);
    }

    [Fact]
    public void ApplyTimeScale_ScalesDeltaTime()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f
        };

        var result = controller.ApplyTimeScale(1.0f);

        Assert.Equal(2.0f, result);
    }

    [Fact]
    public void ApplyTimeScale_ReturnsZeroWhenPaused()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f,
            IsPaused = true
        };

        var result = controller.ApplyTimeScale(1.0f);

        Assert.Equal(0f, result);
    }

    [Fact]
    public void GetScoreMultiplier_NormalSpeed_ReturnsBase()
    {
        var controller = new TimeController
        {
            TimeScale = 1.0f
        };

        var multiplier = controller.GetScoreMultiplier(2.0f);

        Assert.Equal(2.0f, multiplier);
    }

    [Fact]
    public void GetScoreMultiplier_DoubleSpeed_ReturnsHigher()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f
        };

        var multiplier = controller.GetScoreMultiplier(1.0f);

        Assert.Equal(1.5f, multiplier);
    }

    [Fact]
    public void GetScoreMultiplier_TripleSpeed_ReturnsHigher()
    {
        var controller = new TimeController
        {
            TimeScale = 3.0f
        };

        var multiplier = controller.GetScoreMultiplier(1.0f);

        Assert.Equal(2.0f, multiplier);
    }

    [Fact]
    public void GetScoreMultiplier_QuintupleSpeed_ReturnsHighest()
    {
        var controller = new TimeController
        {
            TimeScale = 5.0f
        };

        var multiplier = controller.GetScoreMultiplier(1.0f);

        Assert.Equal(3.0f, multiplier);
    }

    [Fact]
    public void GetScoreMultiplierWithDiminishingReturns_HighSpeed_LowerThanLinear()
    {
        var controller = new TimeController
        {
            TimeScale = 5.0f
        };

        var linearMultiplier = controller.GetScoreMultiplier(1.0f);
        var diminishingMultiplier = controller.GetScoreMultiplierWithDiminishingReturns(1.0f);

        Assert.True(diminishingMultiplier < linearMultiplier);
        Assert.True(diminishingMultiplier > 1.0f);
    }

    [Fact]
    public void SetTimeScalePreset_Normal_SetsToOne()
    {
        var controller = new TimeController();

        controller.SetTimeScalePreset(TimeScalePreset.Normal);

        Assert.Equal(1.0f, controller.TimeScale);
    }

    [Fact]
    public void SetTimeScalePreset_Double_SetsToTwo()
    {
        var controller = new TimeController();

        controller.SetTimeScalePreset(TimeScalePreset.Double);

        Assert.Equal(2.0f, controller.TimeScale);
    }

    [Fact]
    public void SetTimeScalePreset_Paused_SetsPausedState()
    {
        var controller = new TimeController();

        controller.SetTimeScalePreset(TimeScalePreset.Paused);

        Assert.True(controller.IsPaused);
    }

    [Fact]
    public void IncreaseTimeScale_IncrementsByStep()
    {
        var controller = new TimeController
        {
            TimeScale = 1.0f
        };

        controller.IncreaseTimeScale(0.5f);

        Assert.Equal(1.5f, controller.TimeScale);
    }

    [Fact]
    public void DecreaseTimeScale_DecrementsByStep()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f
        };

        controller.DecreaseTimeScale(0.5f);

        Assert.Equal(1.5f, controller.TimeScale);
    }

    [Fact]
    public void TogglePause_TogglesPauseState()
    {
        var controller = new TimeController();

        Assert.False(controller.IsPaused);

        controller.TogglePause();
        Assert.True(controller.IsPaused);

        controller.TogglePause();
        Assert.False(controller.IsPaused);
    }

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var controller = new TimeController
        {
            TimeScale = 3.0f,
            IsPaused = true
        };

        controller.Reset();

        Assert.Equal(1.0f, controller.TimeScale);
        Assert.False(controller.IsPaused);
    }

    [Fact]
    public void GetRealTimeForSimulationTime_DoubleSpeed_ReturnsHalf()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f
        };

        var realTime = controller.GetRealTimeForSimulationTime(TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(5), realTime);
    }

    [Fact]
    public void GetSimulationTimeForRealTime_DoubleSpeed_ReturnsDouble()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f
        };

        var simTime = controller.GetSimulationTimeForRealTime(TimeSpan.FromMinutes(5));

        Assert.Equal(TimeSpan.FromMinutes(10), simTime);
    }

    [Fact]
    public void IsNormalSpeed_AtNormalSpeed_ReturnsTrue()
    {
        var controller = new TimeController
        {
            TimeScale = 1.0f
        };

        Assert.True(controller.IsNormalSpeed);
    }

    [Fact]
    public void IsFasterThanNormal_AtDoubleSpeed_ReturnsTrue()
    {
        var controller = new TimeController
        {
            TimeScale = 2.0f
        };

        Assert.True(controller.IsFasterThanNormal);
        Assert.False(controller.IsSlowerThanNormal);
    }

    [Fact]
    public void IsSlowerThanNormal_AtHalfSpeed_ReturnsTrue()
    {
        var controller = new TimeController
        {
            TimeScale = 0.5f
        };

        Assert.True(controller.IsSlowerThanNormal);
        Assert.False(controller.IsFasterThanNormal);
    }
}
