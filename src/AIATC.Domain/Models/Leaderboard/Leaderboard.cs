namespace AIATC.Domain.Models.Leaderboard;

/// <summary>
/// Complete leaderboard with entries and metadata
/// </summary>
public class Leaderboard
{
    /// <summary>
    /// Type of leaderboard
    /// </summary>
    public LeaderboardType Type { get; set; }

    /// <summary>
    /// Time frame for this leaderboard
    /// </summary>
    public TimeFrame TimeFrame { get; set; }

    /// <summary>
    /// Leaderboard entries
    /// </summary>
    public List<LeaderboardEntry> Entries { get; set; } = new();

    /// <summary>
    /// Current user's entry (may not be in top entries)
    /// </summary>
    public LeaderboardEntry? CurrentUserEntry { get; set; }

    /// <summary>
    /// Total number of users in this leaderboard
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// When this leaderboard was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Checks if there are more pages available
    /// </summary>
    public bool HasNextPage => Page * PageSize < TotalUsers;

    /// <summary>
    /// Checks if there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalUsers / PageSize);
}
