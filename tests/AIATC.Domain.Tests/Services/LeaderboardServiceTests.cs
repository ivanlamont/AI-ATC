using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Leaderboard;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Moq;
using Xunit;

namespace AIATC.Domain.Tests.Services;

public class LeaderboardServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly LeaderboardService _service;
    private readonly List<User> _testUsers;

    public LeaderboardServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _service = new LeaderboardService(_userRepositoryMock.Object);
        _testUsers = CreateTestUsers();
    }

    [Fact]
    public async Task GetLeaderboardAsync_ReturnsOrderedByScore()
    {
        _userRepositoryMock.Setup(r => r.GetActiveUsersAsync()).ReturnsAsync(_testUsers);

        var lb = await _service.GetLeaderboardAsync(LeaderboardType.HighestScore, TimeFrame.AllTime, 1, 10);

        Assert.NotNull(lb);
        Assert.Equal(3, lb.TotalUsers);
        Assert.Equal(LeaderboardType.HighestScore, lb.Type);
        Assert.Equal(TimeFrame.AllTime, lb.TimeFrame);
        Assert.True(lb.Entries[0].Score >= lb.Entries[1].Score);
    }

    [Fact]
    public async Task GetLeaderboardAsync_WithPagination_ReturnsCorrectPage()
    {
        _userRepositoryMock.Setup(r => r.GetActiveUsersAsync()).ReturnsAsync(_testUsers);

        var lb = await _service.GetLeaderboardAsync(LeaderboardType.HighestScore, TimeFrame.AllTime, 1, 1);

        Assert.Single(lb.Entries);
        Assert.Equal(1, lb.Entries[0].Rank);
    }

    [Fact]
    public async Task GetTopUsersAsync_ReturnsTopN()
    {
        _userRepositoryMock.Setup(r => r.GetActiveUsersAsync()).ReturnsAsync(_testUsers);

        var top = await _service.GetTopUsersAsync(LeaderboardType.SkillRating, 2);

        Assert.Equal(2, top.Count);
    }

    [Fact]
    public async Task GetUserRankAsync_ReturnsCorrectRank()
    {
        _userRepositoryMock.Setup(r => r.GetActiveUsersAsync()).ReturnsAsync(_testUsers);

        var userId = _testUsers[1].Id;
        var rank = await _service.GetUserRankAsync(userId, LeaderboardType.HighestScore);

        Assert.NotNull(rank);
        Assert.True(rank > 0);
    }

    private List<User> CreateTestUsers()
    {
        return new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Username = "player1",
                Email = "p1@test.com",
                DisplayName = "Player One",
                IsActive = true,
                LastLoginAt = DateTime.UtcNow,
                Statistics = new UserStatistics
                {
                    HighestScore = 1000,
                    SkillRating = 1500,
                    ScenariosCompleted = 50
                }
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "player2",
                Email = "p2@test.com",
                DisplayName = "Player Two",
                IsActive = true,
                LastLoginAt = DateTime.UtcNow,
                Statistics = new UserStatistics
                {
                    HighestScore = 800,
                    SkillRating = 1300,
                    ScenariosCompleted = 30
                }
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "player3",
                Email = "p3@test.com",
                DisplayName = "Player Three",
                IsActive = true,
                LastLoginAt = DateTime.UtcNow,
                Statistics = new UserStatistics
                {
                    HighestScore = 600,
                    SkillRating = 1100,
                    ScenariosCompleted = 20
                }
            }
        };
    }
}
