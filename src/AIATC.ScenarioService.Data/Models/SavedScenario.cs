using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIATC.ScenarioService.Data.Models;

/// <summary>
/// Represents a saved game state for a scenario
/// </summary>
[Table("saved_scenarios")]
public class SavedScenario
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

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("saved_state")]
    public string SavedState { get; set; } = string.Empty; // JSON saved state

    [Column("saved_at")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    [Column("progress_percentage")]
    public decimal? ProgressPercentage { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ScenarioId))]
    public virtual Scenario Scenario { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public virtual User User { get; set; } = null!;
}
