using System;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Scenarios;

/// <summary>
/// Metadata and description for a scenario
/// </summary>
public class ScenarioMetadata
{
    /// <summary>
    /// Unique identifier for the scenario
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name of the scenario
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the scenario
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Scenario difficulty level
    /// </summary>
    public ScenarioDifficulty Difficulty { get; set; }

    /// <summary>
    /// Expected duration in minutes
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Airport location (ICAO code)
    /// </summary>
    public string LocationId { get; set; } = string.Empty;

    /// <summary>
    /// Airport name for display
    /// </summary>
    public string LocationName { get; set; } = string.Empty;

    /// <summary>
    /// Category tags for filtering
    /// </summary>
    public List<string> Tags { get; set; } = new();

    /// <summary>
    /// Author of the scenario
    /// </summary>
    public string Author { get; set; } = "System";

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Scenario version
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Minimum recommended skill level (1-10)
    /// </summary>
    public int MinimumSkillLevel { get; set; } = 1;

    /// <summary>
    /// Maximum number of aircraft in scenario
    /// </summary>
    public int MaxAircraft { get; set; } = 10;

    /// <summary>
    /// Whether this is a training scenario
    /// </summary>
    public bool IsTraining { get; set; }

    /// <summary>
    /// Preview image URL (optional)
    /// </summary>
    public string? PreviewImageUrl { get; set; }
}

/// <summary>
/// Scenario difficulty levels
/// </summary>
public enum ScenarioDifficulty
{
    /// <summary>
    /// Beginner-friendly with minimal traffic
    /// </summary>
    Easy = 1,

    /// <summary>
    /// Moderate traffic and complexity
    /// </summary>
    Medium = 2,

    /// <summary>
    /// Heavy traffic with challenging conditions
    /// </summary>
    Hard = 3,

    /// <summary>
    /// Expert-level with extreme conditions
    /// </summary>
    Expert = 4,

    /// <summary>
    /// Custom difficulty (user-defined)
    /// </summary>
    Custom = 5
}
