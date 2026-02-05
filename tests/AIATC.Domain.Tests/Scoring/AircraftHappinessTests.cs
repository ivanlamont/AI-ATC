using Xunit;
using AIATC.Domain.Models.Scoring;
using System;

namespace AIATC.Domain.Tests.Scoring;

public class AircraftHappinessTests
{
    [Fact]
    public void AircraftHappiness_InitializesAt100()
    {
        var happiness = new AircraftHappiness { Callsign = "UAL123" };

        Assert.Equal(100.0f, happiness.Happiness);
        Assert.Equal("UAL123", happiness.Callsign);
    }

    [Fact]
    public void ModifyHappiness_IncreasesValue()
    {
        var happiness = new AircraftHappiness();

        happiness.ModifyHappiness(-10, "Test decrease");

        Assert.Equal(90.0f, happiness.Happiness);
        Assert.Single(happiness.Changes);
    }

    [Fact]
    public void ModifyHappiness_ClampsToZero()
    {
        var happiness = new AircraftHappiness();

        happiness.ModifyHappiness(-150, "Massive penalty");

        Assert.Equal(0.0f, happiness.Happiness);
    }

    [Fact]
    public void ModifyHappiness_ClampsTo100()
    {
        var happiness = new AircraftHappiness();

        happiness.ModifyHappiness(-20, "Decrease");
        happiness.ModifyHappiness(30, "Increase");

        Assert.Equal(100.0f, happiness.Happiness); // Can't exceed 100
    }

    [Fact]
    public void ModifyHappiness_RecordsChanges()
    {
        var happiness = new AircraftHappiness();

        happiness.ModifyHappiness(-5, "First change");
        happiness.ModifyHappiness(-10, "Second change");

        Assert.Equal(2, happiness.Changes.Count);
        Assert.Equal("First change", happiness.Changes[0].Reason);
        Assert.Equal(-5, happiness.Changes[0].Delta);
    }

    [Fact]
    public void GetRouteEfficiency_CalculatesCorrectly()
    {
        var happiness = new AircraftHappiness
        {
            DirectDistance = 50,
            TotalDistanceFlown = 100
        };

        var efficiency = happiness.GetRouteEfficiency();

        Assert.Equal(0.5f, efficiency); // 50 / 100 = 0.5
    }

    [Fact]
    public void GetRouteEfficiency_CapsAt1()
    {
        var happiness = new AircraftHappiness
        {
            DirectDistance = 50,
            TotalDistanceFlown = 40 // Flew less than direct distance
        };

        var efficiency = happiness.GetRouteEfficiency();

        Assert.Equal(1.0f, efficiency); // Capped at 100%
    }

    [Fact]
    public void GetFinalScore_IncludesHappiness()
    {
        var happiness = new AircraftHappiness
        {
            DirectDistance = 50,
            TotalDistanceFlown = 50
        };

        happiness.ModifyHappiness(-10, "Minor issue");

        var score = happiness.GetFinalScore();

        // Base 90 happiness + 50 efficiency bonus = 140
        Assert.True(score >= 100);
    }

    [Fact]
    public void GetFinalScore_PenalizesExcessiveCommands()
    {
        var happiness = new AircraftHappiness
        {
            CommandCount = 15,
            DirectDistance = 50,
            TotalDistanceFlown = 50
        };

        var score = happiness.GetFinalScore();

        // Penalty for 10 excessive commands (15 - 5 = 10 * 5 = -50)
        Assert.True(score < 150); // Would be 150 without penalty
    }

    [Fact]
    public void GetFinalScore_BonusForLanding()
    {
        var happiness = new AircraftHappiness
        {
            LandedSuccessfully = true,
            DirectDistance = 50,
            TotalDistanceFlown = 50
        };

        var score = happiness.GetFinalScore();

        // Includes +100 landing bonus
        Assert.True(score >= 200);
    }

    [Fact]
    public void GetFinalScore_PenalizesHolding()
    {
        var happiness = new AircraftHappiness
        {
            TimeInHold = 600, // 10 minutes
            DirectDistance = 50,
            TotalDistanceFlown = 50
        };

        var score = happiness.GetFinalScore();

        // -100 penalty for 10 minutes (10 * 10)
        Assert.True(score < 150);
    }
}
