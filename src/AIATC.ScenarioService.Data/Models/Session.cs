using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIATC.ScenarioService.Data.Models;

/// <summary>
/// Represents a play session of a scenario
/// </summary>
[Table("sessions")]
public class Session
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [Column("scenario_id")]
    public Guid ScenarioId { get; set; }

    [Required]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("started_at")]
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    [Column("ended_at")]
    public DateTime? EndedAt { get; set; }

    [MaxLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active"; // active, completed, abandoned

    [Column("initial_state")]
    public string? InitialState { get; set; } // JSON initial conditions

    [Column("final_state")]
    public string? FinalState { get; set; } // JSON final state

    [Column("score")]
    public int? Score { get; set; }

    [Column("metrics")]
    public string? Metrics { get; set; } // JSON performance metrics

    // Navigation properties
    [ForeignKey(nameof(ScenarioId))]
    public virtual Scenario Scenario { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;

    public virtual ICollection<SessionCommand> Commands { get; set; } = new List<SessionCommand>();
    public virtual ICollection<SessionEvent> Events { get; set; } = new List<SessionEvent>();
}
