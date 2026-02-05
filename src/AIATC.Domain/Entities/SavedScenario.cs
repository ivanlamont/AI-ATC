using System;

namespace AIATC.Domain.Entities;

public class SavedScenario
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ScenarioId { get; set; }
    public string? SaveName { get; set; }
    public byte[] SimulationState { get; set; } = Array.Empty<byte>();
    public float SimulationTime { get; set; }
    public int CurrentScore { get; set; }
    public DateTime SavedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = null!;
    public Scenario Scenario { get; set; } = null!;
}
