using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Moq;
using Xunit;

namespace AIATC.Domain.Tests.Services;

public class DashboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthTokenRepository> _tokenRepositoryMock;
    private readonly Mock<LeaderboardService> _leaderboardServiceMock;
    private readonly DashboardService _service;

    public DashboardServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenRepositoryMock = new Mock<IAuthTokenRepository>();
        _leaderboardServiceMock = new Mock<LeaderboardService>(
            _userRepositoryMock.Object);

        _service = new DashboardService(
            _userRepositoryMock.Object,
            _tokenRepositoryMock.Object,
            _leaderboardServiceMock.Object);
    }

    [Fact]
    public async Task GetSystemMetricsAsync_ReturnsValidMetrics()
    {
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Username = "user1",
                Email = "u1@test.com",
                IsActive = true,
                LastLoginAt = DateTime.UtcNow,
                Statistics = new UserStatistics
                {
                    ScenariosCompleted = 10,
                    AircraftLanded = 5,
                    TotalPlaytimeSeconds = 3600,
                    SkillRating = 1200
                }
            }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var metrics = await _service.GetSystemMetricsAsync();

        Assert.NotNull(metrics);
        Assert.Equal(1, metrics.TotalUsers);
        Assert.Equal(1, metrics.ActiveUsers);
        Assert.Equal(10, metrics.TotalScenariosCompleted);
    }

    [Fact]
    public async Task GetUserActivityAsync_FiltersActiveUsers()
    {
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "active", Email = "a@test.com", IsActive = true },
            new User { Id = Guid.NewGuid(), Username = "inactive", Email = "i@test.com", IsActive = false }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var activity = await _service.GetUserActivityAsync(activeOnly: true);

        Assert.Single(activity);
        Assert.Equal("active", activity[0].Username);
    }

    [Fact]
    public async Task GetNewUsersAsync_ReturnsRecentUsers()
    {
        var now = DateTime.UtcNow;
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "new", Email = "n@test.com", CreatedAt = now.AddDays(-3) },
            new User { Id = Guid.NewGuid(), Username = "old", Email = "o@test.com", CreatedAt = now.AddDays(-15) }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var newUsers = await _service.GetNewUsersAsync(7);

        Assert.Single(newUsers);
        Assert.Equal("new", newUsers[0].Username);
    }

    [Fact]
    public async Task GetInactiveUsersAsync_ReturnsInactiveUsers()
    {
        var now = DateTime.UtcNow;
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Username = "active", Email = "a@test.com", LastLoginAt = now },
            new User { Id = Guid.NewGuid(), Username = "inactive", Email = "i@test.com", LastLoginAt = now.AddDays(-35) }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var inactive = await _service.GetInactiveUsersAsync(30);

        Assert.Single(inactive);
        Assert.Equal("inactive", inactive[0].Username);
    }

    [Fact]
    public async Task GetUserCountByRoleAsync_ReturnsRoleCounts()
    {
        var users = new List<User>
        {
            new User { Roles = new List<UserRole> { UserRole.Administrator, UserRole.User } },
            new User { Roles = new List<UserRole> { UserRole.User } }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        var counts = await _service.GetUserCountByRoleAsync();

        Assert.Equal(1, counts[UserRole.Administrator]);
        Assert.Equal(2, counts[UserRole.User]);
    }

    [Fact]
    public void HasDashboardAccess_WithAdminRole_ReturnsTrue()
    {
        var user = new User { Roles = new List<UserRole> { UserRole.Administrator } };

        var hasAccess = _service.HasDashboardAccess(user);

        Assert.True(hasAccess);
    }

    [Fact]
    public void HasDashboardAccess_WithObserverRole_ReturnsTrue()
    {
        var user = new User { Roles = new List<UserRole> { UserRole.Observer } };

        var hasAccess = _service.HasDashboardAccess(user);

        Assert.True(hasAccess);
    }

    [Fact]
    public void HasManagementPermissions_WithAdminRole_ReturnsTrue()
    {
        var user = new User { Roles = new List<UserRole> { UserRole.Administrator } };

        var hasPermissions = _service.HasManagementPermissions(user);

        Assert.True(hasPermissions);
    }

    [Fact]
    public void HasManagementPermissions_WithoutAdminRole_ReturnsFalse()
    {
        var user = new User { Roles = new List<UserRole> { UserRole.User } };

        var hasPermissions = _service.HasManagementPermissions(user);

        Assert.False(hasPermissions);
    }
}
