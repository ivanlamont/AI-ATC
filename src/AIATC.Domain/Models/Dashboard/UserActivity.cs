using AIATC.Domain.Models.Users;

namespace AIATC.Domain.Models.Dashboard;

/// <summary>
/// User activity summary for dashboard
/// </summary>
public class UserActivity
{
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User roles
    /// </summary>
    public List<UserRole> Roles { get; set; } = new();

    /// <summary>
    /// Whether account is active
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// When user was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Scenarios completed
    /// </summary>
    public int ScenariosCompleted { get; set; }

    /// <summary>
    /// Skill rating
    /// </summary>
    public int SkillRating { get; set; }

    /// <summary>
    /// Total playtime in hours
    /// </summary>
    public double PlaytimeHours { get; set; }

    /// <summary>
    /// Success rate (perfect scenarios / total scenarios)
    /// </summary>
    public float SuccessRate { get; set; }

    /// <summary>
    /// OAuth provider
    /// </summary>
    public string? OAuthProvider { get; set; }

    /// <summary>
    /// Days since last login
    /// </summary>
    public int? DaysSinceLastLogin
    {
        get
        {
            if (LastLoginAt == null) return null;
            return (DateTime.UtcNow - LastLoginAt.Value).Days;
        }
    }

    /// <summary>
    /// Checks if user is new (created within last 7 days)
    /// </summary>
    public bool IsNewUser => (DateTime.UtcNow - CreatedAt).Days <= 7;

    /// <summary>
    /// Checks if user is inactive (no login in 30+ days)
    /// </summary>
    public bool IsInactive => DaysSinceLastLogin >= 30;
}
