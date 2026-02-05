using AIATC.Domain.Data;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Leaderboard;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIATC.Domain.Tests.Integration;

/// <summary>
/// End-to-end integration tests for complete system workflows
/// </summary>
public class EndToEndIntegrationTests : IDisposable
{
    private readonly AircraftControlDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenRepository _tokenRepository;
    private readonly JwtTokenService _jwtService;
    private readonly DatabaseAuthenticationService _authService;
    private readonly LeaderboardService _leaderboardService;
    private readonly DashboardService _dashboardService;

    public EndToEndIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AircraftControlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AircraftControlDbContext(options);
        _userRepository = new UserRepository(_context);
        _tokenRepository = new AuthTokenRepository(_context);

        var config = new JwtConfiguration
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationSeconds = 3600
        };
        _jwtService = new JwtTokenService(config);

        _authService = new DatabaseAuthenticationService(
            _userRepository,
            _tokenRepository,
            _jwtService);

        _leaderboardService = new LeaderboardService(_userRepository);
        _dashboardService = new DashboardService(_userRepository, _tokenRepository, _leaderboardService);
    }

    [Fact]
    public async Task CompleteUserJourney_RegistrationToLeaderboard()
    {
        // Act - Register new user
        var user = await _authService.RegisterOAuthUserAsync(
            "player@example.com",
            "player1",
            "Player One",
            "google",
            "google-id-1");

        Assert.NotNull(user);
        Assert.Equal("player1", user.Username);
        Assert.True(user.EmailVerified);
        Assert.Contains(UserRole.User, user.Roles);

        // Act - Authenticate user
        var authResponse = await _authService.AuthenticateOAuthAsync(
            "player@example.com",
            "google",
            "google-id-1");

        Assert.NotNull(authResponse.AccessToken);
        var tokenValidation = _jwtService.ValidateAccessToken(authResponse.AccessToken);
        Assert.True(tokenValidation.IsValid);

        // Act - Update user statistics
        user.Statistics.HighestScore = 1500;
        user.Statistics.SkillRating = 2000;
        user.Statistics.ScenariosCompleted = 100;
        user.Statistics.AircraftLanded = 50;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act - Check leaderboard position
        var leaderboard = await _leaderboardService.GetLeaderboardAsync(
            LeaderboardType.HighestScore,
            TimeFrame.AllTime,
            1,
            10);

        Assert.NotNull(leaderboard);
        Assert.Single(leaderboard.Entries);
        Assert.Equal(user.Id, leaderboard.Entries[0].UserId);

        // Act - Check user rank
        var rank = await _leaderboardService.GetUserRankAsync(
            user.Id,
            LeaderboardType.HighestScore);

        Assert.Equal(1, rank);

        // Act - Get dashboard metrics
        var metrics = await _dashboardService.GetSystemMetricsAsync();

        Assert.Equal(1, metrics.TotalUsers);
        Assert.Equal(1, metrics.ActiveUsers);
        Assert.Equal(100, metrics.TotalScenariosCompleted);
    }

    [Fact]
    public async Task MultiPlayerCompetition_LeaderboardRanking()
    {
        // Arrange - Register multiple players
        var players = new[]
        {
            ("alice@example.com", "alice", "Alice", 2500),
            ("bob@example.com", "bob", "Bob", 2000),
            ("charlie@example.com", "charlie", "Charlie", 1500),
            ("diana@example.com", "diana", "Diana", 3000),
            ("eve@example.com", "eve", "Eve", 1000),
        };

        var userIds = new List<Guid>();

        // Act - Register and setup all players
        foreach (var (email, username, displayName, score) in players)
        {
            var user = await _authService.RegisterOAuthUserAsync(
                email,
                username,
                displayName,
                "google",
                $"google-{username}");

            user.Statistics.HighestScore = score;
            user.Statistics.SkillRating = score;
            user.Statistics.ScenariosCompleted = score / 100;
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            userIds.Add(user.Id);
        }

        // Act - Get top 3 players by score
        var topPlayers = await _leaderboardService.GetTopUsersAsync(
            LeaderboardType.HighestScore,
            3);

        // Assert - Top 3 should be Diana, Alice, Bob
        Assert.Equal(3, topPlayers.Count);
        Assert.Equal("diana", topPlayers[0].Username);
        Assert.Equal("alice", topPlayers[1].Username);
        Assert.Equal("bob", topPlayers[2].Username);

        // Act - Get full leaderboard
        var leaderboard = await _leaderboardService.GetLeaderboardAsync(
            LeaderboardType.HighestScore,
            TimeFrame.AllTime,
            1,
            5);

        // Assert - All 5 players should be present
        Assert.Equal(5, leaderboard.Entries.Count);
        Assert.Equal(1, leaderboard.Entries[0].Rank);
        Assert.Equal(5, leaderboard.Entries[4].Rank);

        // Assert - Scores should be in descending order
        for (int i = 0; i < leaderboard.Entries.Count - 1; i++)
        {
            Assert.True(leaderboard.Entries[i].Score >= leaderboard.Entries[i + 1].Score);
        }

        // Act - Check dashboard metrics
        var metrics = await _dashboardService.GetSystemMetricsAsync();

        Assert.Equal(5, metrics.TotalUsers);
        Assert.Equal(5, metrics.ActiveUsers);
    }

    [Fact]
    public async Task AdminDashboard_UserManagement()
    {
        // Arrange - Create users with different roles
        var regularUser = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "user",
            "Regular User",
            "google",
            "google-user");

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Username = "admin",
            DisplayName = "Admin User",
            OAuthProvider = "google",
            OAuthProviderId = "google-admin",
            EmailVerified = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Roles = new List<UserRole> { UserRole.Administrator, UserRole.User }
        };

        await _userRepository.AddAsync(adminUser);
        await _userRepository.SaveChangesAsync();

        var observerUser = await _authService.RegisterOAuthUserAsync(
            "observer@example.com",
            "observer",
            "Observer User",
            "google",
            "google-observer");

        // Act - Update observer to have observer role
        observerUser.Roles = new List<UserRole> { UserRole.User, UserRole.Observer };
        await _userRepository.UpdateAsync(observerUser);
        await _userRepository.SaveChangesAsync();

        // Assert - Admin has management permissions
        Assert.True(_dashboardService.HasDashboardAccess(adminUser));
        Assert.True(_dashboardService.HasManagementPermissions(adminUser));

        // Assert - Observer has dashboard access but no management
        var observer = await _userRepository.GetByIdAsync(observerUser.Id);
        Assert.NotNull(observer);
        Assert.True(_dashboardService.HasDashboardAccess(observer));
        Assert.False(_dashboardService.HasManagementPermissions(observer));

        // Assert - Regular user has no access
        Assert.False(_dashboardService.HasDashboardAccess(regularUser));
        Assert.False(_dashboardService.HasManagementPermissions(regularUser));

        // Act - Get user counts by role
        var roleCounts = await _dashboardService.GetUserCountByRoleAsync();

        // Assert - Verify role counts
        Assert.Equal(1, roleCounts[UserRole.Administrator]);
        Assert.Equal(3, roleCounts[UserRole.User]);
        Assert.Equal(1, roleCounts[UserRole.Observer]);
    }

    [Fact]
    public async Task UserDeactivation_ImpactOnLeaderboards()
    {
        // Arrange - Register 3 players
        var player1 = await _authService.RegisterOAuthUserAsync(
            "p1@example.com", "p1", "Player 1", "google", "gp1");

        var player2 = await _authService.RegisterOAuthUserAsync(
            "p2@example.com", "p2", "Player 2", "google", "gp2");

        var player3 = await _authService.RegisterOAuthUserAsync(
            "p3@example.com", "p3", "Player 3", "google", "gp3");

        player1.Statistics.HighestScore = 3000;
        player2.Statistics.HighestScore = 2000;
        player3.Statistics.HighestScore = 1000;

        await _userRepository.UpdateAsync(player1);
        await _userRepository.UpdateAsync(player2);
        await _userRepository.UpdateAsync(player3);
        await _userRepository.SaveChangesAsync();

        // Act - Get active leaderboard
        var activeLB = await _leaderboardService.GetLeaderboardAsync(
            LeaderboardType.HighestScore,
            TimeFrame.AllTime,
            1,
            10);

        Assert.Equal(3, activeLB.Entries.Count);

        // Act - Deactivate player 1
        await _authService.DeactivateUserAsync(player1.Id);

        // Act - Get leaderboard again
        var updatedLB = await _leaderboardService.GetLeaderboardAsync(
            LeaderboardType.HighestScore,
            TimeFrame.AllTime,
            1,
            10);

        // Assert - Deactivated player should not appear
        Assert.Equal(2, updatedLB.Entries.Count);
        Assert.DoesNotContain(player1.Id, updatedLB.Entries.Select(e => e.UserId));
        Assert.Contains(player2.Id, updatedLB.Entries.Select(e => e.UserId));
        Assert.Contains(player3.Id, updatedLB.Entries.Select(e => e.UserId));

        // Act - Check dashboard metrics
        var metrics = await _dashboardService.GetSystemMetricsAsync();

        Assert.Equal(3, metrics.TotalUsers);
        Assert.Equal(2, metrics.ActiveUsers);
    }

    [Fact]
    public async Task TokenCleanup_MaintenanceTask()
    {
        // Arrange - Create user and multiple sessions
        var user = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "user",
            "Test User",
            "google",
            "google-id");

        var session1 = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google-id");

        var session2 = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google-id");

        // Create an expired token
        var expiredToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Type = TokenType.Refresh,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };

        await _tokenRepository.AddAsync(expiredToken);
        await _tokenRepository.SaveChangesAsync();

        // Act - Verify we have 3 tokens before cleanup
        var tokensBefore = await _tokenRepository.GetByUserIdAsync(user.Id);
        Assert.Equal(3, tokensBefore.Count());

        // Act - Cleanup expired tokens
        await _authService.CleanupExpiredTokensAsync();

        // Assert - Only valid tokens should remain
        var tokensAfter = await _tokenRepository.GetByUserIdAsync(user.Id);
        Assert.Equal(2, tokensAfter.Count());
        Assert.DoesNotContain("expired-token", tokensAfter.Select(t => t.Token));
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
