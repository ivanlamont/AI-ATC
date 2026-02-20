using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIATC.ScenarioService.Data.Models;

/// <summary>
/// Represents an event that occurred during a session
/// </summary>
[Table("session_events")]
public class SessionEvent
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("session_id")]
    public Guid SessionId { get; set; }

    [Column("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(50)]
    [Column("event_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("event_data")]
    public string? EventData { get; set; } // JSON event details

    [MaxLength(20)]
    [Column("severity")]
    public string? Severity { get; set; } // info, warning, error, critical

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; } = null!;
}
