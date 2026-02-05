using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Leaderboard;
using Microsoft.EntityFrameworkCore;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for generating and managing leaderboards
/// </summary>
public class LeaderboardService
{
    private readonly IUserRepository _userRepository;

    public LeaderboardService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Gets leaderboard for specified type and time frame
    /// </summary>
    public async Task<Leaderboard> GetLeaderboardAsync(
        LeaderboardType type,
        TimeFrame timeFrame = TimeFrame.AllTime,
        int page = 1,
        int pageSize = 50,
        Guid? currentUserId = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        // Get active users matching time frame
        var users = await GetUsersForTimeFrameAsync(timeFrame);

        // Order users by the specified metric
        var orderedUsers = OrderUsersByType(users, type);

        // Count total users
        var totalUsers = orderedUsers.Count();

        // Get paginated entries
        var entries = orderedUsers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select((user, index) => new LeaderboardEntry
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Rank = (page - 1) * pageSize + index + 1,
                Score = GetScoreForType(user, type),
                SecondaryMetric = GetSecondaryMetricForType(user, type),
                SkillRating = user.Statistics.SkillRating,
                Roles = user.Roles,
                LastLoginAt = user.LastLoginAt
            })
            .ToList();

        // Find current user's entry if specified
        LeaderboardEntry? currentUserEntry = null;
        if (currentUserId.HasValue)
        {
            var currentUserRank = orderedUsers
                .Select((user, index) => new { user.Id, Rank = index + 1 })
                .FirstOrDefault(x => x.Id == currentUserId.Value);

            if (currentUserRank != null)
            {
                var currentUser = orderedUsers.ElementAt(currentUserRank.Rank - 1);
                currentUserEntry = new LeaderboardEntry
                {
                    UserId = currentUser.Id,
                    Username = currentUser.Username,
                    DisplayName = currentUser.DisplayName,
                    AvatarUrl = currentUser.AvatarUrl,
                    Rank = currentUserRank.Rank,
                    Score = GetScoreForType(currentUser, type),
                    SecondaryMetric = GetSecondaryMetricForType(currentUser, type),
                    SkillRating = currentUser.Statistics.SkillRating,
                    Roles = currentUser.Roles,
                    LastLoginAt = currentUser.LastLoginAt
                };
            }
        }

        return new Leaderboard
        {
            Type = type,
            TimeFrame = timeFrame,
            Entries = entries,
            CurrentUserEntry = currentUserEntry,
            TotalUsers = totalUsers,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets top N users for a specific leaderboard type
    /// </summary>
    public async Task<List<LeaderboardEntry>> GetTopUsersAsync(
        LeaderboardType type,
        int count = 10,
        TimeFrame timeFrame = TimeFrame.AllTime)
    {
        var leaderboard = await GetLeaderboardAsync(type, timeFrame, 1, count);
        return leaderboard.Entries;
    }

    /// <summary>
    /// Gets user's rank for a specific leaderboard type
    /// </summary>
    public async Task<int?> GetUserRankAsync(
        Guid userId,
        LeaderboardType type,
        TimeFrame timeFrame = TimeFrame.AllTime)
    {
        var users = await GetUsersForTimeFrameAsync(timeFrame);
        var orderedUsers = OrderUsersByType(users, type);

        var rank = orderedUsers
            .Select((user, index) => new { user.Id, Rank = index + 1 })
            .FirstOrDefault(x => x.Id == userId);

        return rank?.Rank;
    }

    /// <summary>
    /// Gets users around a specific user's rank
    /// </summary>
    public async Task<Leaderboard> GetLeaderboardAroundUserAsync(
        Guid userId,
        LeaderboardType type,
        TimeFrame timeFrame = TimeFrame.AllTime,
        int rangeAbove = 5,
        int rangeBelow = 5)
    {
        var users = await GetUsersForTimeFrameAsync(timeFrame);
        var orderedUsers = OrderUsersByType(users, type).ToList();
        var totalUsers = orderedUsers.Count;

        var userIndex = orderedUsers.FindIndex(u => u.Id == userId);
        if (userIndex == -1)
        {
            // User not found, return empty leaderboard
            return new Leaderboard
            {
                Type = type,
                TimeFrame = timeFrame,
                TotalUsers = totalUsers
            };
        }

        var startIndex = Math.Max(0, userIndex - rangeAbove);
        var endIndex = Math.Min(orderedUsers.Count - 1, userIndex + rangeBelow);
        var count = endIndex - startIndex + 1;

        var entries = orderedUsers
            .Skip(startIndex)
            .Take(count)
            .Select((user, index) => new LeaderboardEntry
            {
                UserId = user.Id,
                Username = user.Username,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Rank = startIndex + index + 1,
                Score = GetScoreForType(user, type),
                SecondaryMetric = GetSecondaryMetricForType(user, type),
                SkillRating = user.Statistics.SkillRating,
                Roles = user.Roles,
                LastLoginAt = user.LastLoginAt
            })
            .ToList();

        var currentUserEntry = entries.FirstOrDefault(e => e.UserId == userId);

        return new Leaderboard
        {
            Type = type,
            TimeFrame = timeFrame,
            Entries = entries,
            CurrentUserEntry = currentUserEntry,
            TotalUsers = totalUsers,
            Page = (userIndex / 50) + 1,
            PageSize = count
        };
    }

    private async Task<IEnumerable<AIATC.Domain.Models.Users.User>> GetUsersForTimeFrameAsync(TimeFrame timeFrame)
    {
        var activeUsers = await _userRepository.GetActiveUsersAsync();

        if (timeFrame == TimeFrame.AllTime)
        {
            return activeUsers;
        }

        var now = DateTime.UtcNow;
        DateTime cutoffDate = timeFrame switch
        {
            TimeFrame.Daily => now.Date,
            TimeFrame.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            TimeFrame.Monthly => new DateTime(now.Year, now.Month, 1),
            _ => DateTime.MinValue
        };

        return activeUsers.Where(u => u.LastLoginAt >= cutoffDate);
    }

    private IEnumerable<AIATC.Domain.Models.Users.User> OrderUsersByType(
        IEnumerable<AIATC.Domain.Models.Users.User> users,
        LeaderboardType type)
    {
        return type switch
        {
            LeaderboardType.HighestScore => users.OrderByDescending(u => u.Statistics.HighestScore)
                                                  .ThenByDescending(u => u.Statistics.ScenariosCompleted),
            LeaderboardType.SkillRating => users.OrderByDescending(u => u.Statistics.SkillRating)
                                                 .ThenByDescending(u => u.Statistics.ScenariosCompleted),
            LeaderboardType.ScenariosCompleted => users.OrderByDescending(u => u.Statistics.ScenariosCompleted)
                                                        .ThenByDescending(u => u.Statistics.HighestScore),
            LeaderboardType.PerfectScenarios => users.OrderByDescending(u => u.Statistics.PerfectScenarios)
                                                      .ThenByDescending(u => u.Statistics.ScenariosCompleted),
            LeaderboardType.AircraftLanded => users.OrderByDescending(u => u.Statistics.AircraftLanded)
                                                    .ThenByDescending(u => u.Statistics.ScenariosCompleted),
            LeaderboardType.CurrentStreak => users.OrderByDescending(u => u.Statistics.CurrentStreak)
                                                   .ThenByDescending(u => u.Statistics.SkillRating),
            LeaderboardType.BestStreak => users.OrderByDescending(u => u.Statistics.BestStreak)
                                                .ThenByDescending(u => u.Statistics.CurrentStreak),
            _ => users.OrderByDescending(u => u.Statistics.HighestScore)
        };
    }

    private int GetScoreForType(AIATC.Domain.Models.Users.User user, LeaderboardType type)
    {
        return type switch
        {
            LeaderboardType.HighestScore => user.Statistics.HighestScore,
            LeaderboardType.SkillRating => user.Statistics.SkillRating,
            LeaderboardType.ScenariosCompleted => user.Statistics.ScenariosCompleted,
            LeaderboardType.PerfectScenarios => user.Statistics.PerfectScenarios,
            LeaderboardType.AircraftLanded => user.Statistics.AircraftLanded,
            LeaderboardType.CurrentStreak => user.Statistics.CurrentStreak,
            LeaderboardType.BestStreak => user.Statistics.BestStreak,
            _ => user.Statistics.HighestScore
        };
    }

    private int GetSecondaryMetricForType(AIATC.Domain.Models.Users.User user, LeaderboardType type)
    {
        return type switch
        {
            LeaderboardType.HighestScore => user.Statistics.ScenariosCompleted,
            LeaderboardType.SkillRating => user.Statistics.ScenariosCompleted,
            LeaderboardType.ScenariosCompleted => user.Statistics.HighestScore,
            LeaderboardType.PerfectScenarios => user.Statistics.ScenariosCompleted,
            LeaderboardType.AircraftLanded => user.Statistics.ScenariosCompleted,
            LeaderboardType.CurrentStreak => user.Statistics.SkillRating,
            LeaderboardType.BestStreak => user.Statistics.CurrentStreak,
            _ => user.Statistics.ScenariosCompleted
        };
    }
}
