using AIATC.Domain.Models.Users;
using Xunit;

namespace AIATC.Domain.Tests.Models.Users;

/// <summary>
/// Tests for User model
/// </summary>
public class UserTests
{
    [Fact]
    public void User_DefaultConstructor_InitializesProperties()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        Assert.NotNull(user.Roles);
        Assert.Empty(user.Roles);
        Assert.True(user.IsActive);
        Assert.False(user.EmailVerified);
        Assert.NotNull(user.Statistics);
        Assert.NotNull(user.Preferences);
    }

    [Fact]
    public void HasRole_WithMatchingRole_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Roles = new List<UserRole> { UserRole.User, UserRole.Premium }
        };

        // Act & Assert
        Assert.True(user.HasRole(UserRole.User));
        Assert.True(user.HasRole(UserRole.Premium));
        Assert.False(user.HasRole(UserRole.Administrator));
    }

    [Fact]
    public void HasAnyRole_WithMatchingRoles_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Roles = new List<UserRole> { UserRole.User }
        };

        // Act & Assert
        Assert.True(user.HasAnyRole(UserRole.User, UserRole.Administrator));
        Assert.True(user.HasAnyRole(UserRole.User));
        Assert.False(user.HasAnyRole(UserRole.Administrator, UserRole.Moderator));
    }

    [Fact]
    public void IsAdmin_WithAdministratorRole_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Roles = new List<UserRole> { UserRole.Administrator }
        };

        // Act & Assert
        Assert.True(user.IsAdmin);
    }

    [Fact]
    public void IsAdmin_WithoutAdministratorRole_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Roles = new List<UserRole> { UserRole.User }
        };

        // Act & Assert
        Assert.False(user.IsAdmin);
    }

    [Fact]
    public void IsModerator_WithModeratorRole_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Roles = new List<UserRole> { UserRole.Moderator }
        };

        // Act & Assert
        Assert.True(user.IsModerator);
    }

    [Fact]
    public void RecordLogin_UpdatesLastLoginAt()
    {
        // Arrange
        var user = new User { LastLoginAt = null };
        var beforeLogin = DateTime.UtcNow;

        // Act
        user.RecordLogin();
        var afterLogin = DateTime.UtcNow;

        // Assert
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= beforeLogin);
        Assert.True(user.LastLoginAt <= afterLogin);
    }

    [Fact]
    public void UserStatistics_GetAverageScore_WithNoScenarios_ReturnsZero()
    {
        // Arrange
        var stats = new UserStatistics
        {
            ScenariosCompleted = 0,
            HighestScore = 1000
        };

        // Act
        var average = stats.GetAverageScore();

        // Assert
        Assert.Equal(0, average);
    }

    [Fact]
    public void UserStatistics_GetAverageScore_WithScenarios_ReturnsAverage()
    {
        // Arrange
        var stats = new UserStatistics
        {
            ScenariosCompleted = 5,
            HighestScore = 1000
        };

        // Act
        var average = stats.GetAverageScore();

        // Assert
        Assert.Equal(200, average);
    }

    [Fact]
    public void UserStatistics_GetSuccessRate_WithNoScenarios_ReturnsZero()
    {
        // Arrange
        var stats = new UserStatistics
        {
            ScenariosCompleted = 0,
            PerfectScenarios = 0
        };

        // Act
        var successRate = stats.GetSuccessRate();

        // Assert
        Assert.Equal(0, successRate);
    }

    [Fact]
    public void UserStatistics_GetSuccessRate_WithScenarios_ReturnsRate()
    {
        // Arrange
        var stats = new UserStatistics
        {
            ScenariosCompleted = 10,
            PerfectScenarios = 7
        };

        // Act
        var successRate = stats.GetSuccessRate();

        // Assert
        Assert.Equal(0.7f, successRate);
    }

    [Fact]
    public void UserPreferences_DefaultConstructor_SetsDefaults()
    {
        // Arrange & Act
        var prefs = new UserPreferences();

        // Assert
        Assert.Equal("dark", prefs.Theme);
        Assert.True(prefs.VoiceCommandsEnabled);
        Assert.True(prefs.TextToSpeechEnabled);
        Assert.Equal(80, prefs.MasterVolume);
        Assert.Equal("Medium", prefs.PreferredDifficulty);
        Assert.True(prefs.ShowTutorials);
        Assert.True(prefs.PublicStatistics);
        Assert.True(prefs.EmailNotifications);
    }
}
