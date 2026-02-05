using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Xunit;

namespace AIATC.Domain.Tests.Services;

/// <summary>
/// Tests for JWT token service
/// </summary>
public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly User _testUser;

    public JwtTokenServiceTests()
    {
        var config = new JwtConfiguration
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            SecretKey = "test-secret-key",
            AccessTokenExpirationSeconds = 3600
        };

        _service = new JwtTokenService(config);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = new List<UserRole> { UserRole.User }
        };
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ReturnsToken()
    {
        // Act
        var token = _service.GenerateAccessToken(_testUser);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.Contains(".", token);
        var parts = token.Split('.');
        Assert.Equal(3, parts.Length);
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserClaims()
    {
        // Act
        var token = _service.GenerateAccessToken(_testUser);

        // Assert - validate by decoding
        var result = _service.ValidateAccessToken(token);
        Assert.True(result.IsValid);
        Assert.Equal(_testUser.Id, result.UserId);
        Assert.True(result.Claims.ContainsKey("email"));
        Assert.True(result.Claims.ContainsKey("unique_name"));
        Assert.True(result.Claims.ContainsKey("display_name"));
    }

    [Fact]
    public void GenerateAccessToken_WithMultipleRoles_IncludesAllRoles()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            Email = "admin@example.com",
            DisplayName = "Admin User",
            Roles = new List<UserRole> { UserRole.User, UserRole.Administrator }
        };

        // Act
        var token = _service.GenerateAccessToken(user);

        // Assert
        var result = _service.ValidateAccessToken(token);
        Assert.True(result.IsValid);
        Assert.True(result.Claims.ContainsKey("roles"));
    }

    [Fact]
    public void ValidateAccessToken_WithValidToken_ReturnsSuccess()
    {
        // Arrange
        var token = _service.GenerateAccessToken(_testUser);

        // Act
        var result = _service.ValidateAccessToken(token);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(_testUser.Id, result.UserId);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateAccessToken_WithInvalidFormat_ReturnsFailure()
    {
        // Arrange
        var invalidToken = "invalid.token";

        // Act
        var result = _service.ValidateAccessToken(invalidToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.UserId);
    }

    [Fact]
    public void ValidateAccessToken_WithMalformedToken_ReturnsFailure()
    {
        // Arrange
        var malformedToken = "header.payload";

        // Act
        var result = _service.ValidateAccessToken(malformedToken);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void ValidateAccessToken_WithExpiredToken_ReturnsFailure()
    {
        // Arrange - create config with very short expiration
        var config = new JwtConfiguration
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationSeconds = -1 // Already expired
        };
        var service = new JwtTokenService(config);
        var token = service.GenerateAccessToken(_testUser);

        // Act
        var result = service.ValidateAccessToken(token);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("expired", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetUserIdFromToken_WithValidToken_ReturnsUserId()
    {
        // Arrange
        var token = _service.GenerateAccessToken(_testUser);

        // Act
        var userId = _service.GetUserIdFromToken(token);

        // Assert
        Assert.NotNull(userId);
        Assert.Equal(_testUser.Id, userId.Value);
    }

    [Fact]
    public void GetUserIdFromToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var userId = _service.GetUserIdFromToken(invalidToken);

        // Assert
        Assert.Null(userId);
    }

    [Fact]
    public void JwtConfiguration_DefaultValues_AreSet()
    {
        // Arrange & Act
        var config = new JwtConfiguration();

        // Assert
        Assert.Equal("AI-ATC", config.Issuer);
        Assert.Equal("AI-ATC-Users", config.Audience);
        Assert.NotNull(config.SecretKey);
        Assert.Equal(3600, config.AccessTokenExpirationSeconds);
        Assert.Equal(2592000, config.RefreshTokenExpirationSeconds);
    }
}
