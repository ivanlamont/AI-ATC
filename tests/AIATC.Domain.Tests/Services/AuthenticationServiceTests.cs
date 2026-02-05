using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Xunit;

namespace AIATC.Domain.Tests.Services;

/// <summary>
/// Tests for authentication service
/// </summary>
public class AuthenticationServiceTests
{
    private readonly AuthenticationService _service;
    private readonly JwtTokenService _jwtService;

    public AuthenticationServiceTests()
    {
        var config = new JwtConfiguration
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationSeconds = 3600
        };
        _jwtService = new JwtTokenService(config);
        _service = new AuthenticationService(_jwtService);
    }

    [Fact]
    public async Task RegisterOAuthUserAsync_WithValidData_CreatesUser()
    {
        // Arrange
        var email = "test@example.com";
        var username = "testuser";
        var displayName = "Test User";

        // Act
        var user = await _service.RegisterOAuthUserAsync(
            email, username, displayName, "google", "google123");

        // Assert
        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(email, user.Email);
        Assert.Equal(username, user.Username);
        Assert.Equal(displayName, user.DisplayName);
        Assert.Equal("google", user.OAuthProvider);
        Assert.Equal("google123", user.OAuthProviderId);
        Assert.True(user.EmailVerified);
        Assert.True(user.IsActive);
        Assert.Contains(UserRole.User, user.Roles);
    }

    [Fact]
    public async Task RegisterOAuthUserAsync_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "user1", "User One", "google", "google123");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.RegisterOAuthUserAsync(
                "test@example.com", "user2", "User Two", "github", "github456"));
    }

    [Fact]
    public async Task RegisterOAuthUserAsync_WithDuplicateUsername_ThrowsException()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "user1@example.com", "testuser", "User One", "google", "google123");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.RegisterOAuthUserAsync(
                "user2@example.com", "testuser", "User Two", "github", "github456"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        // Act
        var response = await _service.AuthenticateOAuthAsync(
            "test@example.com", "google", "google123");

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
    public async Task AuthenticateOAuthAsync_WithInvalidCredentials_ThrowsException()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.AuthenticateOAuthAsync(
                "test@example.com", "google", "wrong-id"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_WithNonexistentUser_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.AuthenticateOAuthAsync(
                "nonexistent@example.com", "google", "google123"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_WithInactiveUser_ThrowsException()
    {
        // Arrange
        var user = await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        await _service.DeactivateUserAsync(user.Id);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.AuthenticateOAuthAsync(
                "test@example.com", "google", "google123"));
    }

    [Fact]
    public async Task AuthenticateOAuthAsync_UpdatesLastLoginAt()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        var beforeLogin = DateTime.UtcNow;

        // Act
        await _service.AuthenticateOAuthAsync(
            "test@example.com", "google", "google123");
        var afterLogin = DateTime.UtcNow;

        // Assert
        var user = _service.GetUserByEmail("test@example.com");
        Assert.NotNull(user);
        Assert.NotNull(user.LastLoginAt);
        Assert.True(user.LastLoginAt >= beforeLogin);
        Assert.True(user.LastLoginAt <= afterLogin);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ReturnsNewAccessToken()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        var authResponse = await _service.AuthenticateOAuthAsync(
            "test@example.com", "google", "google123");
        var originalAccessToken = authResponse.AccessToken;

        // Act
        var refreshResponse = await _service.RefreshTokenAsync(authResponse.RefreshToken);

        // Assert
        Assert.NotNull(refreshResponse);
        Assert.NotNull(refreshResponse.AccessToken);
        Assert.NotEqual(originalAccessToken, refreshResponse.AccessToken);
        Assert.Equal(authResponse.RefreshToken, refreshResponse.RefreshToken);
        Assert.Equal("Bearer", refreshResponse.TokenType);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.RefreshTokenAsync("invalid-refresh-token"));
    }

    [Fact]
    public async Task LogoutAsync_WithValidToken_RevokesToken()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        var authResponse = await _service.AuthenticateOAuthAsync(
            "test@example.com", "google", "google123");

        // Act
        await _service.LogoutAsync(authResponse.RefreshToken);

        // Assert - refresh should fail
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.RefreshTokenAsync(authResponse.RefreshToken));
    }

    [Fact]
    public async Task LogoutAsync_RaisesUserLoggedOutEvent()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        var authResponse = await _service.AuthenticateOAuthAsync(
            "test@example.com", "google", "google123");

        User? loggedOutUser = null;
        _service.UserLoggedOut += (sender, args) => loggedOutUser = args.User;

        // Act
        await _service.LogoutAsync(authResponse.RefreshToken);

        // Assert
        Assert.NotNull(loggedOutUser);
        Assert.Equal("testuser", loggedOutUser.Username);
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Arrange
        var registeredUser = await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        // Act
        var user = _service.GetUser(registeredUser.Id);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(registeredUser.Id, user.Id);
        Assert.Equal("testuser", user.Username);
    }

    [Fact]
    public void GetUser_WithInvalidId_ReturnsNull()
    {
        // Act
        var user = _service.GetUser(Guid.NewGuid());

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task GetUserByEmail_WithValidEmail_ReturnsUser()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        // Act
        var user = _service.GetUserByEmail("test@example.com");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task GetUserByEmail_IsCaseInsensitive()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "Test@Example.Com", "testuser", "Test User", "google", "google123");

        // Act
        var user = _service.GetUserByEmail("test@example.com");

        // Assert
        Assert.NotNull(user);
    }

    [Fact]
    public async Task GetUserByUsername_WithValidUsername_ReturnsUser()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        // Act
        var user = _service.GetUserByUsername("testuser");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("testuser", user.Username);
    }

    [Fact]
    public async Task GetUserByUsername_IsCaseInsensitive()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "TestUser", "Test User", "google", "google123");

        // Act
        var user = _service.GetUserByUsername("testuser");

        // Assert
        Assert.NotNull(user);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WithValidUser_UpdatesRoles()
    {
        // Arrange
        var user = await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        var newRoles = new List<UserRole> { UserRole.User, UserRole.Premium };

        // Act
        await _service.UpdateUserRolesAsync(user.Id, newRoles);

        // Assert
        var updatedUser = _service.GetUser(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(2, updatedUser.Roles.Count);
        Assert.Contains(UserRole.Premium, updatedUser.Roles);
    }

    [Fact]
    public async Task UpdateUserRolesAsync_WithInvalidUser_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.UpdateUserRolesAsync(Guid.NewGuid(), new List<UserRole> { UserRole.User }));
    }

    [Fact]
    public async Task DeactivateUserAsync_WithValidUser_DeactivatesAndRevokesTokens()
    {
        // Arrange
        var user = await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");
        var authResponse = await _service.AuthenticateOAuthAsync(
            "test@example.com", "google", "google123");

        // Act
        await _service.DeactivateUserAsync(user.Id);

        // Assert
        var deactivatedUser = _service.GetUser(user.Id);
        Assert.NotNull(deactivatedUser);
        Assert.False(deactivatedUser.IsActive);

        // Token refresh should fail
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.RefreshTokenAsync(authResponse.RefreshToken));
    }

    [Fact]
    public async Task GetAllUsers_ReturnsAllRegisteredUsers()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "user1@example.com", "user1", "User One", "google", "google1");
        await _service.RegisterOAuthUserAsync(
            "user2@example.com", "user2", "User Two", "github", "github2");

        // Act
        var users = _service.GetAllUsers();

        // Assert
        Assert.Equal(2, users.Count());
    }

    [Fact]
    public async Task UserAuthenticatedEvent_RaisedOnSuccessfulLogin()
    {
        // Arrange
        await _service.RegisterOAuthUserAsync(
            "test@example.com", "testuser", "Test User", "google", "google123");

        User? authenticatedUser = null;
        _service.UserAuthenticated += (sender, args) => authenticatedUser = args.User;

        // Act
        await _service.AuthenticateOAuthAsync("test@example.com", "google", "google123");

        // Assert
        Assert.NotNull(authenticatedUser);
        Assert.Equal("testuser", authenticatedUser.Username);
    }
}
