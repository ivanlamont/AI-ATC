using System;

namespace AIATC.Domain.Entities;

public class SessionCommand
{
    public long Id { get; set; }
    public Guid SessionId { get; set; }
    public float SimulationTime { get; set; }
    public string AircraftId { get; set; } = string.Empty;
    public string CommandType { get; set; } = string.Empty;
    public string CommandText { get; set; } = string.Empty;
    public string? CommandParams { get; set; }  // JSON string
    public int? ResponseTimeMs { get; set; }
    public DateTime IssuedAt { get; set; }

    // Navigation properties
    public Session Session { get; set; } = null!;
}
