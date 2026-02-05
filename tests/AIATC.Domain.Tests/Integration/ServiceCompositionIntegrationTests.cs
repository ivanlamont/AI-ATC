using AIATC.Domain.Data;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Leaderboard;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIATC.Domain.Tests.Integration;

/// <summary>
/// Integration tests for multiple services working together
/// </summary>
public class ServiceCompositionIntegrationTests : IDisposable
{
    private readonly AircraftControlDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly LeaderboardService _leaderboardService;
    private readonly DashboardService _dashboardService;

    public ServiceCompositionIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AircraftControlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AircraftControlDbContext(options);
        _userRepository = new UserRepository(_context);
        _leaderboardService = new LeaderboardService(_userRepository);
        _dashboardService = new DashboardService(_userRepository, null!, _leaderboardService);
    }

    [Fact]
    public async Task LeaderboardAndDashboardService_ConsistentData()
    {
        // Arrange - Create test users
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            Username = "user1",
            DisplayName = "User One",
            IsActive = true,
            Statistics = new UserStatistics
            {
                HighestScore = 1000,
                SkillRating = 1500,
                ScenariosCompleted = 50
            }
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            Username = "user2",
            DisplayName = "User Two",
            IsActive = true,
            Statistics = new UserStatistics
            {
                HighestScore = 800,
                SkillRating = 1300,
                ScenariosCompleted = 40
            }
        };

        var user3 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user3@example.com",
            Username = "user3",
            DisplayName = "User Three",
            IsActive = false,  // Inactive user
            Statistics = new UserStatistics
            {
                HighestScore = 500,
                SkillRating = 1000,
                ScenariosCompleted = 20
            }
        };

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _userRepository.AddAsync(user3);
        await _userRepository.SaveChangesAsync();

        // Act - Get dashboard metrics
        var metrics = await _dashboardService.GetSystemMetricsAsync();

        // Assert - Should count only active users
        Assert.Equal(2, metrics.ActiveUsers);
        Assert.Equal(3, metrics.TotalUsers);

        // Act - Get leaderboard by score
        var leaderboard = await _leaderboardService.GetLeaderboardAsync(
            LeaderboardType.HighestScore,
            TimeFrame.AllTime,
            1,
            10);

        // Assert - Leaderboard should include only active users
        Assert.Equal(2, leaderboard.Entries.Count);
        Assert.Equal(user1.Id, leaderboard.Entries[0].UserId);
        Assert.Equal(user2.Id, leaderboard.Entries[1].UserId);

        // Assert - Scores should be ordered correctly
        Assert.True(leaderboard.Entries[0].Score >= leaderboard.Entries[1].Score);
    }

    [Fact]
    public async Task UserRoleChange_ReflectsInDashboardAccess()
    {
        // Arrange
        var regularUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "user",
            DisplayName = "Regular User",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };

        await _userRepository.AddAsync(regularUser);
        await _userRepository.SaveChangesAsync();

        // Assert - Regular user should not have dashboard access
        Assert.False(_dashboardService.HasDashboardAccess(regularUser));
        Assert.False(_dashboardService.HasManagementPermissions(regularUser));

        // Act - Update user to Observer
        regularUser.Roles = new List<UserRole> { UserRole.User, UserRole.Observer };
        await _userRepository.UpdateAsync(regularUser);
        await _userRepository.SaveChangesAsync();

        var updatedUser = await _userRepository.GetByIdAsync(regularUser.Id);
        Assert.NotNull(updatedUser);

        // Assert - Observer should have dashboard access but not management
        Assert.True(_dashboardService.HasDashboardAccess(updatedUser));
        Assert.False(_dashboardService.HasManagementPermissions(updatedUser));

        // Act - Update user to Administrator
        updatedUser.Roles = new List<UserRole> { UserRole.Administrator };
        await _userRepository.UpdateAsync(updatedUser);
        await _userRepository.SaveChangesAsync();

        var adminUser = await _userRepository.GetByIdAsync(regularUser.Id);
        Assert.NotNull(adminUser);

        // Assert - Admin should have both access and management permissions
        Assert.True(_dashboardService.HasDashboardAccess(adminUser));
        Assert.True(_dashboardService.HasManagementPermissions(adminUser));
    }

    [Fact]
    public async Task LeaderboardFiltering_ByDifferentTimeFrames()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            Username = "user1",
            DisplayName = "User One",
            IsActive = true,
            CreatedAt = now.AddDays(-30),
            LastLoginAt = now,
            Statistics = new UserStatistics
            {
                HighestScore = 1000,
                SkillRating = 1500
            }
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            Username = "user2",
            DisplayName = "User Two",
            IsActive = true,
            CreatedAt = now.AddDays(-2),
            LastLoginAt = now.AddDays(-5),
            Statistics = new UserStatistics
            {
                HighestScore = 800,
                SkillRating = 1300
            }
        };

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _userRepository.SaveChangesAsync();

        // Act - Get all-time leaderboard
        var allTimeLeaderboard = await _leaderboardService.GetLeaderboardAsync(
            LeaderboardType.HighestScore,
            TimeFrame.AllTime,
            1,
            10);

        // Assert - Should include both users
        Assert.Equal(2, allTimeLeaderboard.Entries.Count);
    }

    [Fact]
    public async Task UserActivity_ConsistentAcrossServices()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            IsActive = true,
            LastLoginAt = now,
            Roles = new List<UserRole> { UserRole.User, UserRole.Observer }
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act - Get user activity
        var activities = await _dashboardService.GetUserActivityAsync(activeOnly: false);

        // Assert - Should include the user with correct info
        Assert.Single(activities);
        var userActivity = activities[0];
        Assert.Equal("testuser", userActivity.Username);
        Assert.Equal("Test User", userActivity.DisplayName);
        Assert.NotNull(userActivity.LastLoginAt);
    }

    [Fact]
    public async Task NewUserTracking_AcrossServices()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "new@example.com",
            Username = "newuser",
            DisplayName = "New User",
            IsActive = true,
            CreatedAt = now.AddDays(-3),
            Roles = new List<UserRole> { UserRole.User }
        };

        var oldUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "old@example.com",
            Username = "olduser",
            DisplayName = "Old User",
            IsActive = true,
            CreatedAt = now.AddDays(-15),
            Roles = new List<UserRole> { UserRole.User }
        };

        await _userRepository.AddAsync(newUser);
        await _userRepository.AddAsync(oldUser);
        await _userRepository.SaveChangesAsync();

        // Act
        var newUsers = await _dashboardService.GetNewUsersAsync(7);
        var allUsers = await _dashboardService.GetUserActivityAsync(activeOnly: false);

        // Assert
        Assert.Single(newUsers);
        Assert.Equal("newuser", newUsers[0].Username);
        Assert.Equal(2, allUsers.Count);
    }

    [Fact]
    public async Task InactiveUserDetection_AcrossServices()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var activeUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "active@example.com",
            Username = "activeuser",
            DisplayName = "Active User",
            IsActive = true,
            LastLoginAt = now,
            Roles = new List<UserRole> { UserRole.User }
        };

        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            Username = "inactiveuser",
            DisplayName = "Inactive User",
            IsActive = true,
            LastLoginAt = now.AddDays(-35),
            Roles = new List<UserRole> { UserRole.User }
        };

        await _userRepository.AddAsync(activeUser);
        await _userRepository.AddAsync(inactiveUser);
        await _userRepository.SaveChangesAsync();

        // Act
        var inactiveUsers = await _dashboardService.GetInactiveUsersAsync(30);
        var allActivityUsers = await _dashboardService.GetUserActivityAsync(activeOnly: false);

        // Assert
        Assert.Single(inactiveUsers);
        Assert.Equal("inactiveuser", inactiveUsers[0].Username);
        Assert.Equal(2, allActivityUsers.Count);
    }

    [Fact]
    public async Task UserCountByRole_AcrossMultipleUsers()
    {
        // Arrange
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Username = "admin",
            DisplayName = "Admin",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.Administrator, UserRole.User }
        };

        var observer = new User
        {
            Id = Guid.NewGuid(),
            Email = "observer@example.com",
            Username = "observer",
            DisplayName = "Observer",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.Observer, UserRole.User }
        };

        var regular = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "user",
            DisplayName = "Regular User",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };

        await _userRepository.AddAsync(admin);
        await _userRepository.AddAsync(observer);
        await _userRepository.AddAsync(regular);
        await _userRepository.SaveChangesAsync();

        // Act
        var counts = await _dashboardService.GetUserCountByRoleAsync();

        // Assert
        Assert.Equal(1, counts[UserRole.Administrator]);
        Assert.Equal(1, counts[UserRole.Observer]);
        Assert.Equal(3, counts[UserRole.User]);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
