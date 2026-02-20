using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AIATC.ScenarioService.Data.Models;

/// <summary>
/// Represents a scenario/training exercise configuration
/// </summary>
[Table("scenarios")]
public class Scenario
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(4)]
    [Column("airport_code")]
    public string AirportCode { get; set; } = string.Empty;

    [MaxLength(20)]
    [Column("difficulty")]
    public string? Difficulty { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("configuration")]
    public string? Configuration { get; set; } // JSON configuration

    [Column("objectives")]
    public string? Objectives { get; set; } // JSON objectives

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
    public virtual ICollection<SavedScenario> SavedScenarios { get; set; } = new List<SavedScenario>();
    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
}
