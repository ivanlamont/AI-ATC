using System;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Scenarios;

/// <summary>
/// Represents a scenario objective or goal
/// </summary>
public class ScenarioObjective
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Objective type
    /// </summary>
    public ObjectiveType Type { get; set; }

    /// <summary>
    /// Target value for objective
    /// </summary>
    public float TargetValue { get; set; }

    /// <summary>
    /// Current progress towards objective
    /// </summary>
    public float CurrentValue { get; set; }

    /// <summary>
    /// Whether the objective is complete
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Whether this is a required objective
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Points awarded for completion
    /// </summary>
    public int Points { get; set; } = 100;

    /// <summary>
    /// Additional parameters for objective evaluation
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Gets completion percentage (0-100)
    /// </summary>
    public float GetCompletionPercentage()
    {
        if (TargetValue == 0)
            return IsCompleted ? 100f : 0f;

        return Math.Clamp((CurrentValue / TargetValue) * 100f, 0f, 100f);
    }

    /// <summary>
    /// Updates progress towards objective
    /// </summary>
    public void UpdateProgress(float newValue)
    {
        CurrentValue = newValue;

        if (CurrentValue >= TargetValue && !IsCompleted)
        {
            IsCompleted = true;
        }
    }
}

/// <summary>
/// Types of scenario objectives
/// </summary>
public enum ObjectiveType
{
    /// <summary>
    /// Land a specific number of aircraft
    /// </summary>
    LandAircraft,

    /// <summary>
    /// Maintain separation for duration
    /// </summary>
    MaintainSeparation,

    /// <summary>
    /// Achieve target score
    /// </summary>
    AchieveScore,

    /// <summary>
    /// Complete in time limit
    /// </summary>
    TimeLimit,

    /// <summary>
    /// Handle specific number of aircraft
    /// </summary>
    HandleAircraftCount,

    /// <summary>
    /// Maintain efficiency rating
    /// </summary>
    MaintainEfficiency,

    /// <summary>
    /// Complete without violations
    /// </summary>
    NoViolations,

    /// <summary>
    /// Handle emergency situation
    /// </summary>
    HandleEmergency,

    /// <summary>
    /// Perform successful handoffs
    /// </summary>
    PerformHandoffs,

    /// <summary>
    /// Maintain fuel efficiency
    /// </summary>
    FuelEfficiency,

    /// <summary>
    /// Custom objective (user-defined)
    /// </summary>
    Custom
}
