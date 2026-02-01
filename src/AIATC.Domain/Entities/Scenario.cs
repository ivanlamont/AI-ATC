using System;
using System.Collections.Generic;

namespace AIATC.Domain.Entities;

public class Scenario
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DifficultyLevel { get; set; }
    public string AirportCode { get; set; } = string.Empty;
    public string ScenarioType { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
    public int? MaxAircraft { get; set; }
    public string? WeatherConditions { get; set; }  // JSON string
    public string? InitialAircraftStates { get; set; }  // JSON string
    public string[]? ActiveRunways { get; set; }
    public string? ActiveFrequencies { get; set; }  // JSON string
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsPublic { get; set; }
    public int PlayCount { get; set; }
    public float? AverageScore { get; set; }
    public string[]? Tags { get; set; }

    // Navigation properties
    public User? Creator { get; set; }
    public ICollection<Score> Scores { get; set; } = new List<Score>();
    public ICollection<SavedScenario> SavedProgress { get; set; } = new List<SavedScenario>();
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}
