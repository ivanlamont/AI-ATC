namespace AIATC.Domain.Models.Dashboard;

/// <summary>
/// System-wide metrics for management dashboard
/// </summary>
public class SystemMetrics
{
    /// <summary>
    /// Total registered users
    /// </summary>
    public int TotalUsers { get; set; }

    /// <summary>
    /// Active users (not deactivated)
    /// </summary>
    public int ActiveUsers { get; set; }

    /// <summary>
    /// Users who logged in today
    /// </summary>
    public int UsersLoggedInToday { get; set; }

    /// <summary>
    /// Users who logged in this week
    /// </summary>
    public int UsersLoggedInThisWeek { get; set; }

    /// <summary>
    /// Users who logged in this month
    /// </summary>
    public int UsersLoggedInThisMonth { get; set; }

    /// <summary>
    /// Total scenarios completed across all users
    /// </summary>
    public int TotalScenariosCompleted { get; set; }

    /// <summary>
    /// Total aircraft landed across all users
    /// </summary>
    public int TotalAircraftLanded { get; set; }

    /// <summary>
    /// Total playtime in hours
    /// </summary>
    public double TotalPlaytimeHours { get; set; }

    /// <summary>
    /// Average skill rating across all users
    /// </summary>
    public double AverageSkillRating { get; set; }

    /// <summary>
    /// When these metrics were calculated
    /// </summary>
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
