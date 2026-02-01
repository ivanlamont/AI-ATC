using System;
using System.Collections.Generic;

namespace AIATC.Domain.Entities;

public class Session
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? ScenarioId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public int? Score { get; set; }
    public int AircraftControlled { get; set; }
    public int CommandsIssued { get; set; }
    public int SeparationViolations { get; set; }
    public int SuccessfulLandings { get; set; }
    public int SuccessfulHandoffs { get; set; }
    public float TimeAcceleration { get; set; } = 1.0f;
    public string? FinalScoreBreakdown { get; set; }  // JSON string
    public byte[]? StateSnapshot { get; set; }
    public string Status { get; set; } = "active";

    // Navigation properties
    public User User { get; set; } = null!;
    public Scenario? Scenario { get; set; }
    public ICollection<SessionCommand> Commands { get; set; } = new List<SessionCommand>();
    public ICollection<SessionEvent> Events { get; set; } = new List<SessionEvent>();
}
