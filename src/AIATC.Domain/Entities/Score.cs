using System;

namespace AIATC.Domain.Entities;

public class Score
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid ScenarioId { get; set; }
    public int ScoreValue { get; set; }
    public float TimeAcceleration { get; set; } = 1.0f;
    public int AdjustedScore { get; set; }
    public int AircraftControlled { get; set; }
    public int CommandsIssued { get; set; }
    public float? EfficiencyRating { get; set; }
    public float? SafetyRating { get; set; }
    public DateTime AchievedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Session? Session { get; set; }
    public Scenario Scenario { get; set; } = null!;
}
