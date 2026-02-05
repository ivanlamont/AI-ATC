using System;

namespace AIATC.Domain.Entities;

public class SessionEvent
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public float SimulationTime { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string[]? AircraftIds { get; set; }
    public string? EventData { get; set; }  // JSON string
    public DateTime OccurredAt { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
}
