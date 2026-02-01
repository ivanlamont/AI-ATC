namespace AIATC.Domain.Models.Leaderboard;

/// <summary>
/// Types of leaderboards based on different metrics
/// </summary>
public enum LeaderboardType
{
    /// <summary>
    /// Highest score achieved
    /// </summary>
    HighestScore,

    /// <summary>
    /// Skill rating (ELO-style)
    /// </summary>
    SkillRating,

    /// <summary>
    /// Total scenarios completed
    /// </summary>
    ScenariosCompleted,

    /// <summary>
    /// Perfect scenarios (zero violations)
    /// </summary>
    PerfectScenarios,

    /// <summary>
    /// Total aircraft landed
    /// </summary>
    AircraftLanded,

    /// <summary>
    /// Current win streak
    /// </summary>
    CurrentStreak,

    /// <summary>
    /// Best win streak ever
    /// </summary>
    BestStreak
}
