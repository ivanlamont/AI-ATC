namespace AIATC.Domain.Models.Leaderboard;

/// <summary>
/// Time frames for leaderboard filtering
/// </summary>
public enum TimeFrame
{
    /// <summary>
    /// All-time leaderboard (no time restriction)
    /// </summary>
    AllTime,

    /// <summary>
    /// Daily leaderboard (today only)
    /// </summary>
    Daily,

    /// <summary>
    /// Weekly leaderboard (current week)
    /// </summary>
    Weekly,

    /// <summary>
    /// Monthly leaderboard (current month)
    /// </summary>
    Monthly
}
