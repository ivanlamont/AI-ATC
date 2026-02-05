using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Moq;
using Xunit;

namespace AIATC.Domain.Tests.Services;

/// <summary>
/// Tests for DatabaseAuthenticationService
/// </summary>
public class DatabaseAuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IAuthTokenRepository> _tokenRepositoryMock;
    private readonly JwtTokenService _jwtService;
    private readonly DatabaseAuthenticationService _service;

    public DatabaseAuthenticationServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _tokenRepositoryMock = new Mock<IAuthTokenRepository>();

        var config = new JwtConfiguration
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationSeconds = 3600
        };
        _jwtService = new JwtTokenService(config);

        _service = new DatabaseAuthenticationService(
            _userRepositoryMock.Object,
            _tokenRepositoryMock.Object,
            _jwtService);
    }

    [Fact]
    public async Task RegisterOAuthUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("test@example.com")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("testuser")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>())).ReturnsAsync((User u) => u);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var user = await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("testuser", user.Username);
        Assert.Equal("Test User", user.DisplayName);
        Assert.True(user.EmailVerified);
        Assert.Contains(UserRole.User, user.Roles);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RegisterOAuthUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("test@example.com")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.RegisterOAuthUserAsync(
                "test@example.com", "testuser", "Test User", "google", "google123"));
    }

    [Fact]
    public async Task RegisterOAuthUserAsync_WithDuplicateUsername_ThrowsException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.EmailExistsAsync("test@example.com")).ReturnsAsync(false);
        _userRepositoryMock.Setup(r => r.UsernameExistsAsync("testuser")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.RegisterOAuthUserAsync(
                "test@example.com", "testuser", "Test User", "google", "google123"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            OAuthProvider = "google",
            OAuthProviderId = "google123",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };

        _userRepositoryMock.Setup(r => r.GetByOAuthAsync("google", "google123")).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _tokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuthToken>())).ReturnsAsync((AuthToken t) => t);
        _tokenRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var response = await _service.AuthenticateOAuthAsync("test@example.com", "google", "google123");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.RefreshToken);
        Assert.Equal("Bearer", response.TokenType);
        Assert.Equal(3600, response.ExpiresIn);
        Assert.NotNull(response.User);
        Assert.Equal("testuser", response.User.Username);
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_WithInvalidProvider_ThrowsException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByOAuthAsync("google", "invalid")).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.AuthenticateOAuthAsync("test@example.com", "google", "invalid"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_WithInactiveUser_ThrowsException()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            OAuthProvider = "google",
            OAuthProviderId = "google123",
            IsActive = false
        };

        _userRepositoryMock.Setup(r => r.GetByOAuthAsync("google", "google123")).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.AuthenticateOAuthAsync("test@example.com", "google", "google123"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_RaisesUserAuthenticatedEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            OAuthProvider = "google",
            OAuthProviderId = "google123",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };

        _userRepositoryMock.Setup(r => r.GetByOAuthAsync("google", "google123")).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _tokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<AuthToken>())).ReturnsAsync((AuthToken t) => t);
        _tokenRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        User? authenticatedUser = null;
        _service.UserAuthenticated += (sender, args) => authenticatedUser = args.User;

        // Act
        await _service.AuthenticateOAuthAsync("test@example.com", "google", "google123");

        // Assert
        Assert.NotNull(authenticatedUser);
        Assert.Equal("testuser", authenticatedUser.Username);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            Roles = new List<UserRole> { UserRole.User }
        };

        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "refresh-token",
            Type = TokenType.Refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("refresh-token")).ReturnsAsync(token);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var response = await _service.RefreshTokenAsync("refresh-token");

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.AccessToken);
        Assert.Equal("Bearer", response.TokenType);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsException()
    {
        // Arrange
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("invalid")).ReturnsAsync((AuthToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.RefreshTokenAsync("invalid"));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ThrowsException()
    {
        // Arrange
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "expired-token",
            Type = TokenType.Refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("expired-token")).ReturnsAsync(token);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.RefreshTokenAsync("expired-token"));
    }

    [Fact]
    public async Task RefreshTokenAsync_WithRevokedToken_ThrowsException()
    {
        // Arrange
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Token = "revoked-token",
            Type = TokenType.Refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = true
        };

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("revoked-token")).ReturnsAsync(token);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.RefreshTokenAsync("revoked-token"));
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_RevokesToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser" };
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "refresh-token",
            Type = TokenType.Refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("refresh-token")).ReturnsAsync(token);
        _tokenRepositoryMock.Setup(r => r.UpdateAsync(token)).Returns(Task.CompletedTask);
        _tokenRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        await _service.LogoutAsync("refresh-token");

        // Assert
        _tokenRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AuthToken>()), Times.Once);
        _tokenRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_RaisesUserLoggedOutEvent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "testuser" };
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = "refresh-token",
            Type = TokenType.Refresh,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("refresh-token")).ReturnsAsync(token);
        _tokenRepositoryMock.Setup(r => r.UpdateAsync(token)).Returns(Task.CompletedTask);
        _tokenRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        User? loggedOutUser = null;
        _service.UserLoggedOut += (sender, args) => loggedOutUser = args.User;

        // Act
        await _service.LogoutAsync("refresh-token");

        // Assert
        Assert.NotNull(loggedOutUser);
        Assert.Equal("testuser", loggedOutUser.Username);
    }

    [Fact]
    public async Task GetUserAsync_WithValidId_ReturnsUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", Username = "testuser" };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
    }

    [Fact]
    public async Task GetUserByEmailAsync_WithValidEmail_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _userRepositoryMock.Setup(r => r.GetByEmailAsync("test@example.com")).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_WithValidUsername_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _userRepositoryMock.Setup(r => r.GetByUsernameAsync("testuser")).ReturnsAsync(user);

        // Act
        var result = await _service.GetUserByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WithValidUser_UpdatesRoles()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            Username = "testuser",
            Roles = new List<UserRole> { UserRole.User }
        };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.UpdateUserRolesAsync(userId, new List<UserRole> { UserRole.User, UserRole.Premium });

        // Assert
        Assert.Equal(2, user.Roles.Count);
        Assert.Contains(UserRole.Premium, user.Roles);
        _userRepositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WithInvalidUser_ThrowsException()
    {
        // Arrange
        _userRepositoryMock.Setup(r => r.GetByIdAsync(Guid.NewGuid())).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UpdateUserRolesAsync(Guid.NewGuid(), new List<UserRole> { UserRole.User }));
    }

    [Fact]
    public async Task DeactivateUserAsync_WithValidUser_DeactivatesAndRevokesTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", IsActive = true };

        _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);
        _userRepositoryMock.Setup(r => r.UpdateAsync(user)).Returns(Task.CompletedTask);
        _userRepositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _tokenRepositoryMock.Setup(r => r.RevokeAllUserTokensAsync(userId)).Returns(Task.CompletedTask);

        // Act
        await _service.DeactivateUserAsync(userId);

        // Assert
        Assert.False(user.IsActive);
        _tokenRepositoryMock.Verify(r => r.RevokeAllUserTokensAsync(userId), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com", Username = "user1" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com", Username = "user2" }
        };

        _userRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _service.GetAllUsersAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnsOnlyActiveUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "active@example.com", IsActive = true }
        };

        _userRepositoryMock.Setup(r => r.GetActiveUsersAsync()).ReturnsAsync(users);

        // Act
        var result = await _service.GetActiveUsersAsync();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ReturnsUsersWithRole()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Email = "admin@example.com",
                Roles = new List<UserRole> { UserRole.Administrator }
            }
        };

        _userRepositoryMock.Setup(r => r.GetByRoleAsync(UserRole.Administrator)).ReturnsAsync(users);

        // Act
        var result = await _service.GetUsersByRoleAsync(UserRole.Administrator);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task CleanupExpiredTokensAsync_CallsRepository()
    {
        // Arrange
        _tokenRepositoryMock.Setup(r => r.DeleteExpiredTokensAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.CleanupExpiredTokensAsync();

        // Assert
        _tokenRepositoryMock.Verify(r => r.DeleteExpiredTokensAsync(), Times.Once);
    }
}
