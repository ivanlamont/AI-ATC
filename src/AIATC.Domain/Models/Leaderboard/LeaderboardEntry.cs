using AIATC.Domain.Models.Users;

namespace AIATC.Domain.Models.Leaderboard;

/// <summary>
/// Represents a single entry in a leaderboard
/// </summary>
public class LeaderboardEntry
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
    /// Avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Rank position (1-based)
    /// </summary>
    public int Rank { get; set; }

    /// <summary>
    /// Score value for this leaderboard type
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Secondary metric (e.g., scenarios completed, perfect scenarios)
    /// </summary>
    public int SecondaryMetric { get; set; }

    /// <summary>
    /// Skill rating
    /// </summary>
    public int SkillRating { get; set; }

    /// <summary>
    /// User roles
    /// </summary>
    public List<UserRole> Roles { get; set; } = new();

    /// <summary>
    /// When this user last logged in
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Checks if entry represents current user
    /// </summary>
    public bool IsCurrentUser(Guid currentUserId)
    {
        return UserId == currentUserId;
    }
}
