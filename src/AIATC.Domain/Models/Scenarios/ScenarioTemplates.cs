using AIATC.Domain.Models.Weather;
using AIATC.Domain.Services;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Scenarios;

/// <summary>
/// Factory for creating predefined scenario templates
/// </summary>
public static class ScenarioTemplates
{
    /// <summary>
    /// Creates a beginner training scenario
    /// </summary>
    public static Scenario CreateBeginnerTraining(string locationId = "KJFK", string locationName = "John F. Kennedy International")
    {
        return new Scenario
        {
            Metadata = new ScenarioMetadata
            {
                Id = "beginner-training",
                Name = "First Day on the Job",
                Description = "Learn the basics of air traffic control with light traffic and clear weather.",
                Difficulty = ScenarioDifficulty.Easy,
                DurationMinutes = 15,
                LocationId = locationId,
                LocationName = locationName,
                Tags = new List<string> { "training", "beginner", "tutorial" },
                IsTraining = true,
                MinimumSkillLevel = 1,
                MaxAircraft = 5
            },
            Configuration = new ScenarioConfiguration
            {
                AircraftConfig = new AircraftSpawnConfig
                {
                    InitialAircraftCount = 2,
                    MaximumAircraftCount = 5,
                    SpawnRatePerMinute = 0.5f,
                    ArrivalPercentage = 100f, // All arrivals
                    MinSpawnDistanceNm = 30f,
                    MaxSpawnDistanceNm = 50f
                },
                WeatherConfig = new WeatherConfig
                {
                    Difficulty = WeatherDifficulty.Easy,
                    DynamicWeather = false
                },
                AirspaceConfig = new AirspaceConfig
                {
                    ControlledAirspaceRadiusNm = 60f,
                    MinimumSeparationNm = 3f,
                    HandoffsEnabled = false
                },
                SimulationConfig = new SimulationConfig
                {
                    TimeScale = 1.0f,
                    AiAssistanceEnabled = true,
                    ShowWarnings = true
                },
                ScoringConfig = new ScoringConfig
                {
                    ScoringEnabled = true,
                    DifficultyMultiplier = 1.0f,
                    TargetScore = 500
                }
            },
            Objectives = new List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Name = "Land 5 Aircraft",
                    Description = "Successfully land 5 aircraft at the destination airport",
                    Type = ObjectiveType.LandAircraft,
                    TargetValue = 5,
                    IsRequired = true,
                    Points = 500
                },
                new ScenarioObjective
                {
                    Name = "No Violations",
                    Description = "Maintain proper separation throughout the scenario",
                    Type = ObjectiveType.NoViolations,
                    TargetValue = 1,
                    IsRequired = false,
                    Points = 200
                }
            }
        };
    }

    /// <summary>
    /// Creates a moderate difficulty rush hour scenario
    /// </summary>
    public static Scenario CreateRushHour(string locationId = "KJFK", string locationName = "John F. Kennedy International")
    {
        return new Scenario
        {
            Metadata = new ScenarioMetadata
            {
                Id = "rush-hour",
                Name = "Rush Hour",
                Description = "Handle busy traffic during peak hours with moderate weather conditions.",
                Difficulty = ScenarioDifficulty.Medium,
                DurationMinutes = 30,
                LocationId = locationId,
                LocationName = locationName,
                Tags = new List<string> { "busy", "moderate", "peak" },
                IsTraining = false,
                MinimumSkillLevel = 4,
                MaxAircraft = 12
            },
            Configuration = new ScenarioConfiguration
            {
                AircraftConfig = new AircraftSpawnConfig
                {
                    InitialAircraftCount = 6,
                    MaximumAircraftCount = 12,
                    SpawnRatePerMinute = 1.5f,
                    ArrivalPercentage = 60f,
                    MinSpawnDistanceNm = 35f,
                    MaxSpawnDistanceNm = 65f
                },
                WeatherConfig = new WeatherConfig
                {
                    Difficulty = WeatherDifficulty.Medium,
                    DynamicWeather = true,
                    UpdateIntervalSeconds = 60f
                },
                AirspaceConfig = new AirspaceConfig
                {
                    ControlledAirspaceRadiusNm = 60f,
                    MinimumSeparationNm = 3f,
                    HandoffsEnabled = true
                },
                SimulationConfig = new SimulationConfig
                {
                    TimeScale = 1.0f,
                    AiAssistanceEnabled = false,
                    ShowWarnings = true
                },
                ScoringConfig = new ScoringConfig
                {
                    ScoringEnabled = true,
                    DifficultyMultiplier = 1.5f,
                    TargetScore = 1500,
                    BonusForEfficiency = true
                }
            },
            Objectives = new List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Name = "Land 15 Aircraft",
                    Description = "Successfully land 15 aircraft",
                    Type = ObjectiveType.LandAircraft,
                    TargetValue = 15,
                    IsRequired = true,
                    Points = 1000
                },
                new ScenarioObjective
                {
                    Name = "Time Limit",
                    Description = "Complete within 30 minutes",
                    Type = ObjectiveType.TimeLimit,
                    TargetValue = 1800f, // 30 minutes in seconds
                    IsRequired = true,
                    Points = 500
                },
                new ScenarioObjective
                {
                    Name = "Minimal Violations",
                    Description = "Complete with 2 or fewer violations",
                    Type = ObjectiveType.NoViolations,
                    TargetValue = 1,
                    IsRequired = false,
                    Points = 300
                }
            }
        };
    }

    /// <summary>
    /// Creates a challenging storm scenario
    /// </summary>
    public static Scenario CreateStormChallenge(string locationId = "KJFK", string locationName = "John F. Kennedy International")
    {
        return new Scenario
        {
            Metadata = new ScenarioMetadata
            {
                Id = "storm-challenge",
                Name = "Storm Challenge",
                Description = "Navigate aircraft through severe weather with strong winds and low visibility.",
                Difficulty = ScenarioDifficulty.Hard,
                DurationMinutes = 20,
                LocationId = locationId,
                LocationName = locationName,
                Tags = new List<string> { "weather", "challenging", "storm" },
                IsTraining = false,
                MinimumSkillLevel = 7,
                MaxAircraft = 10
            },
            Configuration = new ScenarioConfiguration
            {
                AircraftConfig = new AircraftSpawnConfig
                {
                    InitialAircraftCount = 4,
                    MaximumAircraftCount = 10,
                    SpawnRatePerMinute = 1.0f,
                    ArrivalPercentage = 80f,
                    MinSpawnDistanceNm = 40f,
                    MaxSpawnDistanceNm = 60f
                },
                WeatherConfig = new WeatherConfig
                {
                    Difficulty = WeatherDifficulty.Hard,
                    DynamicWeather = true,
                    UpdateIntervalSeconds = 45f
                },
                AirspaceConfig = new AirspaceConfig
                {
                    ControlledAirspaceRadiusNm = 60f,
                    MinimumSeparationNm = 5f, // Increased for safety
                    HandoffsEnabled = true
                },
                SimulationConfig = new SimulationConfig
                {
                    TimeScale = 1.0f,
                    AiAssistanceEnabled = false,
                    ShowWarnings = true
                },
                ScoringConfig = new ScoringConfig
                {
                    ScoringEnabled = true,
                    DifficultyMultiplier = 2.0f,
                    TargetScore = 2000,
                    PenalizeSeparationViolations = true
                }
            },
            Objectives = new List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Name = "Land 12 Aircraft",
                    Description = "Successfully land 12 aircraft in severe weather",
                    Type = ObjectiveType.LandAircraft,
                    TargetValue = 12,
                    IsRequired = true,
                    Points = 1500
                },
                new ScenarioObjective
                {
                    Name = "Perfect Separation",
                    Description = "Complete with zero violations",
                    Type = ObjectiveType.NoViolations,
                    TargetValue = 1,
                    IsRequired = false,
                    Points = 500
                },
                new ScenarioObjective
                {
                    Name = "High Efficiency",
                    Description = "Maintain 80% efficiency rating",
                    Type = ObjectiveType.MaintainEfficiency,
                    TargetValue = 80f,
                    IsRequired = false,
                    Points = 300
                }
            }
        };
    }

    /// <summary>
    /// Creates an expert-level scenario
    /// </summary>
    public static Scenario CreateExpertChallenge(string locationId = "KJFK", string locationName = "John F. Kennedy International")
    {
        return new Scenario
        {
            Metadata = new ScenarioMetadata
            {
                Id = "expert-challenge",
                Name = "Expert Challenge",
                Description = "The ultimate test: extreme weather, heavy traffic, and multiple emergencies.",
                Difficulty = ScenarioDifficulty.Expert,
                DurationMinutes = 40,
                LocationId = locationId,
                LocationName = locationName,
                Tags = new List<string> { "expert", "extreme", "challenge" },
                IsTraining = false,
                MinimumSkillLevel = 9,
                MaxAircraft = 15
            },
            Configuration = new ScenarioConfiguration
            {
                AircraftConfig = new AircraftSpawnConfig
                {
                    InitialAircraftCount = 8,
                    MaximumAircraftCount = 15,
                    SpawnRatePerMinute = 2.0f,
                    ArrivalPercentage = 50f,
                    MinSpawnDistanceNm = 30f,
                    MaxSpawnDistanceNm = 70f
                },
                WeatherConfig = new WeatherConfig
                {
                    Difficulty = WeatherDifficulty.Extreme,
                    DynamicWeather = true,
                    UpdateIntervalSeconds = 30f
                },
                AirspaceConfig = new AirspaceConfig
                {
                    ControlledAirspaceRadiusNm = 60f,
                    MinimumSeparationNm = 3f,
                    VerticalSeparationFt = 1000f,
                    HandoffsEnabled = true
                },
                SimulationConfig = new SimulationConfig
                {
                    TimeScale = 1.0f,
                    AiAssistanceEnabled = false,
                    CollisionAvoidanceEnabled = false, // No safety net
                    ShowWarnings = true
                },
                ScoringConfig = new ScoringConfig
                {
                    ScoringEnabled = true,
                    DifficultyMultiplier = 3.0f,
                    TargetScore = 3000,
                    PenalizeSeparationViolations = true,
                    BonusForEfficiency = true
                }
            },
            Objectives = new List<ScenarioObjective>
            {
                new ScenarioObjective
                {
                    Name = "Land 25 Aircraft",
                    Description = "Successfully land 25 aircraft in extreme conditions",
                    Type = ObjectiveType.LandAircraft,
                    TargetValue = 25,
                    IsRequired = true,
                    Points = 2000
                },
                new ScenarioObjective
                {
                    Name = "Time Limit",
                    Description = "Complete within 40 minutes",
                    Type = ObjectiveType.TimeLimit,
                    TargetValue = 2400f,
                    IsRequired = true,
                    Points = 500
                },
                new ScenarioObjective
                {
                    Name = "Minimal Violations",
                    Description = "Complete with 3 or fewer violations",
                    Type = ObjectiveType.NoViolations,
                    TargetValue = 1,
                    IsRequired = false,
                    Points = 800
                },
                new ScenarioObjective
                {
                    Name = "Target Score",
                    Description = "Achieve 3000 points",
                    Type = ObjectiveType.AchieveScore,
                    TargetValue = 3000,
                    IsRequired = false,
                    Points = 500
                }
            }
        };
    }

    /// <summary>
    /// Gets all template scenarios
    /// </summary>
    public static List<Scenario> GetAllTemplates(string locationId = "KJFK", string locationName = "John F. Kennedy International")
    {
        return new List<Scenario>
        {
            CreateBeginnerTraining(locationId, locationName),
            CreateRushHour(locationId, locationName),
            CreateStormChallenge(locationId, locationName),
            CreateExpertChallenge(locationId, locationName)
        };
    }
}
