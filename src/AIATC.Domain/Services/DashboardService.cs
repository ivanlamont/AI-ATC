using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Dashboard;
using AIATC.Domain.Models.Leaderboard;
using AIATC.Domain.Models.Users;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for management observation dashboard
/// Provides system metrics, user activity, and management functions
/// </summary>
public class DashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenRepository _tokenRepository;
    private readonly LeaderboardService _leaderboardService;

    public DashboardService(
        IUserRepository userRepository,
        IAuthTokenRepository tokenRepository,
        LeaderboardService leaderboardService)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _leaderboardService = leaderboardService;
    }

    /// <summary>
    /// Gets system-wide metrics
    /// Requires: Administrator or Observer role
    /// </summary>
    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        var allUsers = (await _userRepository.GetAllAsync()).ToList();
        var now = DateTime.UtcNow;

        var totalUsers = allUsers.Count;
        var activeUsers = allUsers.Count(u => u.IsActive);

        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var usersLoggedInToday = allUsers.Count(u => u.LastLoginAt >= todayStart);
        var usersLoggedInThisWeek = allUsers.Count(u => u.LastLoginAt >= weekStart);
        var usersLoggedInThisMonth = allUsers.Count(u => u.LastLoginAt >= monthStart);

        var totalScenariosCompleted = allUsers.Sum(u => u.Statistics.ScenariosCompleted);
        var totalAircraftLanded = allUsers.Sum(u => u.Statistics.AircraftLanded);
        var totalPlaytimeSeconds = allUsers.Sum(u => u.Statistics.TotalPlaytimeSeconds);
        var averageSkillRating = activeUsers > 0 ? allUsers.Where(u => u.IsActive).Average(u => u.Statistics.SkillRating) : 0;

        return new SystemMetrics
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            UsersLoggedInToday = usersLoggedInToday,
            UsersLoggedInThisWeek = usersLoggedInThisWeek,
            UsersLoggedInThisMonth = usersLoggedInThisMonth,
            TotalScenariosCompleted = totalScenariosCompleted,
            TotalAircraftLanded = totalAircraftLanded,
            TotalPlaytimeHours = totalPlaytimeSeconds / 3600.0,
            AverageSkillRating = averageSkillRating
        };
    }

    /// <summary>
    /// Gets user activity list with filtering and sorting
    /// Requires: Administrator or Observer role
    /// </summary>
    public async Task<List<UserActivity>> GetUserActivityAsync(
        bool? activeOnly = null,
        UserRole? roleFilter = null,
        int? daysInactive = null,
        string? searchTerm = null)
    {
        var users = await _userRepository.GetAllAsync();

        // Apply filters
        if (activeOnly.HasValue && activeOnly.Value)
        {
            users = users.Where(u => u.IsActive);
        }

        if (roleFilter.HasValue)
        {
            users = users.Where(u => u.Roles.Contains(roleFilter.Value));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            users = users.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search) ||
                u.DisplayName.ToLower().Contains(search));
        }

        var activities = users.Select(u => new UserActivity
        {
            UserId = u.Id,
            Username = u.Username,
            DisplayName = u.DisplayName,
            Email = u.Email,
            Roles = u.Roles,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            LastLoginAt = u.LastLoginAt,
            ScenariosCompleted = u.Statistics.ScenariosCompleted,
            SkillRating = u.Statistics.SkillRating,
            PlaytimeHours = u.Statistics.TotalPlaytimeSeconds / 3600.0,
            SuccessRate = u.Statistics.GetSuccessRate(),
            OAuthProvider = u.OAuthProvider
        }).ToList();

        // Apply inactivity filter
        if (daysInactive.HasValue)
        {
            activities = activities.Where(a =>
                a.DaysSinceLastLogin.HasValue &&
                a.DaysSinceLastLogin.Value >= daysInactive.Value).ToList();
        }

        // Sort by last login (most recent first)
        return activities.OrderByDescending(a => a.LastLoginAt ?? DateTime.MinValue).ToList();
    }

    /// <summary>
    /// Gets new users (registered within specified days)
    /// Requires: Administrator or Observer role
    /// </summary>
    public async Task<List<UserActivity>> GetNewUsersAsync(int withinDays = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-withinDays);
        var users = await _userRepository.GetAllAsync();

        return users
            .Where(u => u.CreatedAt >= cutoffDate)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserActivity
            {
                UserId = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Roles = u.Roles,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                ScenariosCompleted = u.Statistics.ScenariosCompleted,
                SkillRating = u.Statistics.SkillRating,
                PlaytimeHours = u.Statistics.TotalPlaytimeSeconds / 3600.0,
                SuccessRate = u.Statistics.GetSuccessRate(),
                OAuthProvider = u.OAuthProvider
            })
            .ToList();
    }

    /// <summary>
    /// Gets inactive users (no login within specified days)
    /// Requires: Administrator or Observer role
    /// </summary>
    public async Task<List<UserActivity>> GetInactiveUsersAsync(int inactiveDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-inactiveDays);
        var users = await _userRepository.GetAllAsync();

        return users
            .Where(u => u.LastLoginAt < cutoffDate || u.LastLoginAt == null)
            .OrderBy(u => u.LastLoginAt ?? DateTime.MinValue)
            .Select(u => new UserActivity
            {
                UserId = u.Id,
                Username = u.Username,
                DisplayName = u.DisplayName,
                Email = u.Email,
                Roles = u.Roles,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                ScenariosCompleted = u.Statistics.ScenariosCompleted,
                SkillRating = u.Statistics.SkillRating,
                PlaytimeHours = u.Statistics.TotalPlaytimeSeconds / 3600.0,
                SuccessRate = u.Statistics.GetSuccessRate(),
                OAuthProvider = u.OAuthProvider
            })
            .ToList();
    }

    /// <summary>
    /// Gets top performers across multiple metrics
    /// Requires: Administrator or Observer role
    /// </summary>
    public async Task<Dictionary<string, List<LeaderboardEntry>>> GetTopPerformersAsync(int count = 10)
    {
        var topPerformers = new Dictionary<string, List<LeaderboardEntry>>();

        // Get top performers for each metric
        topPerformers["HighestScore"] = await _leaderboardService.GetTopUsersAsync(LeaderboardType.HighestScore, count);
        topPerformers["SkillRating"] = await _leaderboardService.GetTopUsersAsync(LeaderboardType.SkillRating, count);
        topPerformers["ScenariosCompleted"] = await _leaderboardService.GetTopUsersAsync(LeaderboardType.ScenariosCompleted, count);
        topPerformers["PerfectScenarios"] = await _leaderboardService.GetTopUsersAsync(LeaderboardType.PerfectScenarios, count);

        return topPerformers;
    }

    /// <summary>
    /// Gets user count by role
    /// Requires: Administrator or Observer role
    /// </summary>
    public async Task<Dictionary<UserRole, int>> GetUserCountByRoleAsync()
    {
        var users = await _userRepository.GetAllAsync();
        var roleCounts = new Dictionary<UserRole, int>();

        foreach (UserRole role in Enum.GetValues(typeof(UserRole)))
        {
            roleCounts[role] = users.Count(u => u.Roles.Contains(role));
        }

        return roleCounts;
    }

    /// <summary>
    /// Gets active token count for monitoring
    /// Requires: Administrator role
    /// </summary>
    public async Task<int> GetActiveTokenCountAsync()
    {
        var allTokens = await _tokenRepository.GetAllAsync();
        return allTokens.Count(t => t.IsValid);
    }

    /// <summary>
    /// Performs system cleanup (expired tokens)
    /// Requires: Administrator role
    /// </summary>
    public async Task PerformSystemCleanupAsync()
    {
        await _tokenRepository.DeleteExpiredTokensAsync();
    }

    /// <summary>
    /// Checks if user has dashboard access (Admin or Observer)
    /// </summary>
    public bool HasDashboardAccess(User user)
    {
        return user.HasAnyRole(UserRole.Administrator, UserRole.Observer);
    }

    /// <summary>
    /// Checks if user has management permissions (Admin only)
    /// </summary>
    public bool HasManagementPermissions(User user)
    {
        return user.HasRole(UserRole.Administrator);
    }
}
