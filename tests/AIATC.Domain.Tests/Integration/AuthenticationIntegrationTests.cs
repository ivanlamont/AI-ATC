using AIATC.Domain.Data;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIATC.Domain.Tests.Integration;

/// <summary>
/// Integration tests for authentication flow across services
/// </summary>
public class AuthenticationIntegrationTests : IDisposable
{
    private readonly AircraftControlDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenRepository _tokenRepository;
    private readonly JwtTokenService _jwtService;
    private readonly DatabaseAuthenticationService _authService;

    public AuthenticationIntegrationTests()
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
    }

    [Fact]
    public async Task CompleteAuthenticationFlow_RegistrationToLogout()
    {
        // Arrange & Act - Registration
        var user = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "testuser",
            "Test User",
            "google",
            "google123",
            "https://example.com/avatar.jpg");

        Assert.NotNull(user);
        Assert.Equal("user@example.com", user.Email);

        // Act - Authentication
        var authResponse = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        Assert.NotNull(authResponse.AccessToken);
        Assert.NotNull(authResponse.RefreshToken);

        // Act - Validate token
        var tokenValidation = _jwtService.ValidateAccessToken(authResponse.AccessToken);
        Assert.True(tokenValidation.IsValid);
        Assert.Equal(user.Id, tokenValidation.UserId);

        // Act - Refresh token
        var refreshResponse = await _authService.RefreshTokenAsync(authResponse.RefreshToken);
        Assert.NotNull(refreshResponse.AccessToken);
        Assert.NotEqual(authResponse.AccessToken, refreshResponse.AccessToken);

        // Act - Logout
        await _authService.LogoutAsync(authResponse.RefreshToken);

        // Assert - Token should be invalid after logout
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _authService.RefreshTokenAsync(authResponse.RefreshToken));
    }

    [Fact]
    public async Task MultipleAuthenticationSessions_TokenManagement()
    {
        // Arrange
        var user = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "testuser",
            "Test User",
            "google",
            "google123");

        // Act - Create first session
        var session1 = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        // Act - Create second session
        var session2 = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        // Assert - Both sessions should be valid
        Assert.NotEqual(session1.RefreshToken, session2.RefreshToken);

        var token1Validation = _jwtService.ValidateAccessToken(session1.AccessToken);
        var token2Validation = _jwtService.ValidateAccessToken(session2.AccessToken);

        Assert.True(token1Validation.IsValid);
        Assert.True(token2Validation.IsValid);

        // Act - Get all valid tokens
        var validTokens = await _tokenRepository.GetValidTokensByUserIdAsync(user.Id);
        Assert.Equal(2, validTokens.Count());

        // Act - Revoke all tokens
        await _tokenRepository.RevokeAllUserTokensAsync(user.Id);

        // Assert - All tokens should be revoked
        var revokedTokens = await _tokenRepository.GetByUserIdAsync(user.Id);
        Assert.All(revokedTokens, t => Assert.True(t.IsRevoked));
    }

    [Fact]
    public async Task TokenExpiration_CleanupAndValidation()
    {
        // Arrange
        var user = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "testuser",
            "Test User",
            "google",
            "google123");

        var authResponse = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        // Create an expired token manually
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

        // Act
        var allTokens = await _tokenRepository.GetByUserIdAsync(user.Id);
        var validTokens = await _tokenRepository.GetValidTokensByUserIdAsync(user.Id);

        Assert.Equal(2, allTokens.Count()); // Valid + Expired
        Assert.Single(validTokens);         // Only valid

        // Act - Cleanup expired tokens
        await _tokenRepository.DeleteExpiredTokensAsync();

        // Assert - Expired token should be deleted
        var remainingTokens = await _tokenRepository.GetByUserIdAsync(user.Id);
        Assert.Single(remainingTokens);
    }

    [Fact]
    public async Task UserRoleUpdate_AffectsAuthentication()
    {
        // Arrange
        var user = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "testuser",
            "Test User",
            "google",
            "google123");

        Assert.Single(user.Roles);
        Assert.Contains(UserRole.User, user.Roles);

        // Act - Authenticate as regular user
        var userAuth = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        var userToken = _jwtService.ValidateAccessToken(userAuth.AccessToken);
        Assert.True(userToken.IsValid);

        // Act - Update roles to admin
        await _authService.UpdateUserRolesAsync(user.Id,
            new List<UserRole> { UserRole.User, UserRole.Administrator });

        // Assert - User should now have admin role
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Contains(UserRole.Administrator, updatedUser.Roles);

        // Act - New authentication should include new roles
        var adminAuth = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        var adminToken = _jwtService.ValidateAccessToken(adminAuth.AccessToken);
        Assert.True(adminToken.IsValid);
    }

    [Fact]
    public async Task UserDeactivation_InvalidatesAllTokens()
    {
        // Arrange
        var user = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "testuser",
            "Test User",
            "google",
            "google123");

        var session1 = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        var session2 = await _authService.AuthenticateOAuthAsync(
            "user@example.com",
            "google",
            "google123");

        // Act - Deactivate user
        await _authService.DeactivateUserAsync(user.Id);

        // Assert - All tokens should be revoked
        var validTokens = await _tokenRepository.GetValidTokensByUserIdAsync(user.Id);
        Assert.Empty(validTokens);

        // Assert - User should be inactive
        var deactivatedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.NotNull(deactivatedUser);
        Assert.False(deactivatedUser.IsActive);

        // Assert - Cannot authenticate with inactive user
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _authService.AuthenticateOAuthAsync(
                "user@example.com",
                "google",
                "google123"));
    }

    [Fact]
    public async Task DifferentOAuthProviders_SameEmailNotAllowed()
    {
        // Arrange & Act - Register with Google
        var googleUser = await _authService.RegisterOAuthUserAsync(
            "user@example.com",
            "googleuser",
            "Google User",
            "google",
            "google123");

        Assert.NotNull(googleUser);
        Assert.Equal("user@example.com", googleUser.Email);

        // Act - Try to register same email with GitHub - should fail
        // Note: The system prevents duplicate emails regardless of OAuth provider
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _authService.RegisterOAuthUserAsync(
                "user@example.com",
                "githubuser",
                "GitHub User",
                "github",
                "github456"));
    }

    [Fact]
    public async Task DifferentEmails_SameOAuthProvider()
    {
        // Arrange & Act - Register first user with Google
        var user1 = await _authService.RegisterOAuthUserAsync(
            "user1@example.com",
            "user1",
            "User One",
            "google",
            "google1");

        // Act - Register second user with same provider but different email
        var user2 = await _authService.RegisterOAuthUserAsync(
            "user2@example.com",
            "user2",
            "User Two",
            "google",
            "google2");

        // Assert - Both users should be created successfully
        Assert.NotEqual(user1.Id, user2.Id);
        Assert.Equal("google", user1.OAuthProvider);
        Assert.Equal("google", user2.OAuthProvider);

        // Act - Authenticate with each user
        var auth1 = await _authService.AuthenticateOAuthAsync(
            "user1@example.com",
            "google",
            "google1");

        var auth2 = await _authService.AuthenticateOAuthAsync(
            "user2@example.com",
            "google",
            "google2");

        // Assert - Should get different tokens for different users
        Assert.NotEqual(auth1.User.Id, auth2.User.Id);

        var token1Validation = _jwtService.ValidateAccessToken(auth1.AccessToken);
        var token2Validation = _jwtService.ValidateAccessToken(auth2.AccessToken);

        Assert.True(token1Validation.IsValid);
        Assert.True(token2Validation.IsValid);
        Assert.NotEqual(token1Validation.UserId, token2Validation.UserId);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
