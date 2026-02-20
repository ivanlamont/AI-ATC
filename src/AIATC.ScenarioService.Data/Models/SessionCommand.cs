using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIATC.ScenarioService.Data.Models;

/// <summary>
/// Represents a command issued during a session
/// </summary>
[Table("session_commands")]
public class SessionCommand
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
    [Column("command_type")]
    public string CommandType { get; set; } = string.Empty;

    [Column("command_data")]
    public string? CommandData { get; set; } // JSON command details

    [MaxLength(100)]
    [Column("target")]
    public string? Target { get; set; } // e.g., callsign

    [Column("success")]
    public bool? Success { get; set; }

    [MaxLength(500)]
    [Column("result")]
    public string? Result { get; set; }

    // Navigation properties
    [ForeignKey(nameof(SessionId))]
    public virtual Session Session { get; set; } = null!;
}
