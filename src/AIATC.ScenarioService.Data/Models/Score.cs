using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIATC.ScenarioService.Data.Models;

/// <summary>
/// Represents a score entry for leaderboards
/// </summary>
[Table("scores")]
public class Score
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

    [Column("score_value")]
    public int ScoreValue { get; set; }

    [Column("completed_at")]
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    [Column("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [Column("details")]
    public string? Details { get; set; } // JSON score details/breakdown

    [Column("rank")]
    public int? Rank { get; set; } // Computed rank

    // Navigation properties
    [ForeignKey(nameof(ScenarioId))]
    public virtual Scenario Scenario { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
