using AIATC.Domain.Models.Scenarios;
using System.Linq;
using Xunit;

namespace AIATC.Domain.Tests.Scenarios;

public class ScenarioTemplatesTests
{
    [Fact]
    public void CreateBeginnerTraining_HasCorrectDifficulty()
    {
        var scenario = ScenarioTemplates.CreateBeginnerTraining();

        Assert.Equal(ScenarioDifficulty.Easy, scenario.Metadata.Difficulty);
        Assert.True(scenario.Metadata.IsTraining);
        Assert.Equal(5, scenario.Configuration.AircraftConfig.MaximumAircraftCount);
    }

    [Fact]
    public void CreateBeginnerTraining_HasObjectives()
    {
        var scenario = ScenarioTemplates.CreateBeginnerTraining();

        Assert.NotEmpty(scenario.Objectives);
        Assert.Contains(scenario.Objectives, o => o.Type == ObjectiveType.LandAircraft);
    }

    [Fact]
    public void CreateRushHour_HasCorrectDifficulty()
    {
        var scenario = ScenarioTemplates.CreateRushHour();

        Assert.Equal(ScenarioDifficulty.Medium, scenario.Metadata.Difficulty);
        Assert.False(scenario.Metadata.IsTraining);
        Assert.Equal(12, scenario.Configuration.AircraftConfig.MaximumAircraftCount);
    }

    [Fact]
    public void CreateRushHour_HasMultipleObjectives()
    {
        var scenario = ScenarioTemplates.CreateRushHour();

        Assert.True(scenario.Objectives.Count >= 2);
        Assert.Contains(scenario.Objectives, o => o.Type == ObjectiveType.LandAircraft);
        Assert.Contains(scenario.Objectives, o => o.Type == ObjectiveType.TimeLimit);
    }

    [Fact]
    public void CreateStormChallenge_HasCorrectDifficulty()
    {
        var scenario = ScenarioTemplates.CreateStormChallenge();

        Assert.Equal(ScenarioDifficulty.Hard, scenario.Metadata.Difficulty);
        Assert.Contains("weather", scenario.Metadata.Tags);
        Assert.True(scenario.Configuration.WeatherConfig.DynamicWeather);
    }

    [Fact]
    public void CreateStormChallenge_HasHigherSeparation()
    {
        var scenario = ScenarioTemplates.CreateStormChallenge();

        Assert.Equal(5f, scenario.Configuration.AirspaceConfig.MinimumSeparationNm);
    }

    [Fact]
    public void CreateExpertChallenge_HasCorrectDifficulty()
    {
        var scenario = ScenarioTemplates.CreateExpertChallenge();

        Assert.Equal(ScenarioDifficulty.Expert, scenario.Metadata.Difficulty);
        Assert.Equal(15, scenario.Configuration.AircraftConfig.MaximumAircraftCount);
        Assert.False(scenario.Configuration.SimulationConfig.CollisionAvoidanceEnabled);
    }

    [Fact]
    public void CreateExpertChallenge_HasHighDifficultyMultiplier()
    {
        var scenario = ScenarioTemplates.CreateExpertChallenge();

        Assert.Equal(3.0f, scenario.Configuration.ScoringConfig.DifficultyMultiplier);
        Assert.Equal(3000, scenario.Configuration.ScoringConfig.TargetScore);
    }

    [Fact]
    public void CreateExpertChallenge_HasManyObjectives()
    {
        var scenario = ScenarioTemplates.CreateExpertChallenge();

        Assert.True(scenario.Objectives.Count >= 4);
        Assert.Contains(scenario.Objectives, o => o.Type == ObjectiveType.AchieveScore);
    }

    [Fact]
    public void GetAllTemplates_ReturnsAllFour()
    {
        var templates = ScenarioTemplates.GetAllTemplates();

        Assert.Equal(4, templates.Count);
        Assert.Contains(templates, s => s.Metadata.Id == "beginner-training");
        Assert.Contains(templates, s => s.Metadata.Id == "rush-hour");
        Assert.Contains(templates, s => s.Metadata.Id == "storm-challenge");
        Assert.Contains(templates, s => s.Metadata.Id == "expert-challenge");
    }

    [Fact]
    public void GetAllTemplates_CustomLocation_UsesLocation()
    {
        var templates = ScenarioTemplates.GetAllTemplates("KLAX", "Los Angeles");

        Assert.All(templates, s =>
        {
            Assert.Equal("KLAX", s.Metadata.LocationId);
            Assert.Equal("Los Angeles", s.Metadata.LocationName);
        });
    }

    [Fact]
    public void AllTemplates_HaveUniqueIds()
    {
        var templates = ScenarioTemplates.GetAllTemplates();
        var ids = templates.Select(t => t.Metadata.Id).ToList();

        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AllTemplates_HaveValidConfiguration()
    {
        var templates = ScenarioTemplates.GetAllTemplates();

        Assert.All(templates, s =>
        {
            Assert.NotNull(s.Configuration);
            Assert.NotNull(s.Configuration.AircraftConfig);
            Assert.NotNull(s.Configuration.WeatherConfig);
            Assert.NotNull(s.Configuration.AirspaceConfig);
            Assert.NotNull(s.Configuration.SimulationConfig);
            Assert.NotNull(s.Configuration.ScoringConfig);
        });
    }
}
