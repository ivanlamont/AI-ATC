using System;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Scoring;

/// <summary>
/// Tracks happiness/satisfaction for an individual aircraft
/// </summary>
public class AircraftHappiness
{
    /// <summary>
    /// Aircraft callsign
    /// </summary>
    public string Callsign { get; set; } = string.Empty;

    /// <summary>
    /// Current happiness value (0-100)
    /// </summary>
    public float Happiness { get; private set; } = 100.0f;

    /// <summary>
    /// Spawn time
    /// </summary>
    public DateTime SpawnTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time of last clearance
    /// </summary>
    public DateTime? LastClearanceTime { get; set; }

    /// <summary>
    /// Number of commands received
    /// </summary>
    public int CommandCount { get; set; }

    /// <summary>
    /// Total distance flown (NM)
    /// </summary>
    public float TotalDistanceFlown { get; set; }

    /// <summary>
    /// Direct distance from spawn to destination (NM)
    /// </summary>
    public float DirectDistance { get; set; }

    /// <summary>
    /// Time spent in holding patterns (seconds)
    /// </summary>
    public float TimeInHold { get; set; }

    /// <summary>
    /// Whether the aircraft landed successfully
    /// </summary>
    public bool LandedSuccessfully { get; set; }

    /// <summary>
    /// History of happiness changes
    /// </summary>
    public List<HappinessChange> Changes { get; set; } = new();

    /// <summary>
    /// Modifies happiness by a delta value
    /// </summary>
    public void ModifyHappiness(float delta, string reason)
    {
        var oldHappiness = Happiness;
        Happiness = Math.Clamp(Happiness + delta, 0, 100);

        Changes.Add(new HappinessChange
        {
            Timestamp = DateTime.UtcNow,
            OldValue = oldHappiness,
            NewValue = Happiness,
            Delta = delta,
            Reason = reason
        });
    }

    /// <summary>
    /// Calculates route efficiency (direct distance / actual distance)
    /// </summary>
    public float GetRouteEfficiency()
    {
        if (TotalDistanceFlown <= 0) return 1.0f;
        return Math.Min(1.0f, DirectDistance / TotalDistanceFlown);
    }

    /// <summary>
    /// Calculates time in air (seconds)
    /// </summary>
    public float GetTimeInAir()
    {
        return (float)(DateTime.UtcNow - SpawnTime).TotalSeconds;
    }

    /// <summary>
    /// Gets final happiness score for scoring purposes
    /// </summary>
    public int GetFinalScore()
    {
        // Base score from happiness (0-100)
        var baseScore = (int)Happiness;

        // Bonus for efficiency (up to +50)
        var efficiencyBonus = (int)(GetRouteEfficiency() * 50);

        // Penalty for excessive commands (diminishing returns)
        var commandPenalty = Math.Max(0, (CommandCount - 5) * 5);

        // Penalty for time in hold
        var holdPenalty = (int)(TimeInHold / 60) * 10; // -10 per minute

        var finalScore = baseScore + efficiencyBonus - commandPenalty - holdPenalty;

        // Landing bonus
        if (LandedSuccessfully)
        {
            finalScore += 100;
        }

        return Math.Max(0, finalScore);
    }
}

/// <summary>
/// Records a change in happiness
/// </summary>
public class HappinessChange
{
    public DateTime Timestamp { get; set; }
    public float OldValue { get; set; }
    public float NewValue { get; set; }
    public float Delta { get; set; }
    public string Reason { get; set; } = string.Empty;
}
