using AIATC.Domain.Data;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIATC.Domain.Tests.Data;

/// <summary>
/// Tests for AuthTokenRepository
/// </summary>
public class AuthTokenRepositoryTests
{
    private readonly AircraftControlDbContext _context;
    private readonly AuthTokenRepository _repository;
    private readonly UserRepository _userRepository;

    public AuthTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AircraftControlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AircraftControlDbContext(options);
        _repository = new AuthTokenRepository(_context);
        _userRepository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByTokenAsync_WithValidToken_ReturnsToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "test-token-123",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(token);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByTokenAsync("test-token-123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token.Id, result.Id);
        Assert.Equal("test-token-123", result.Token);
    }

    [Fact]
    public async Task GetByTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByTokenAsync("nonexistent-token");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserIdAsync_ReturnsAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token1 = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "token-1",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        var token2 = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "token-2",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        var otherUserId = Guid.NewGuid();
        var otherToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = otherUserId,
            Type = TokenType.Refresh,
            Token = "token-3",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(token1);
        await _repository.AddAsync(token2);
        await _repository.AddAsync(otherToken);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAsync(userId);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, t => Assert.Equal(userId, t.UserId));
    }

    [Fact]
    public async Task GetValidTokensByUserIdAsync_ReturnOnlyValidTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var validToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        var expiredToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            IsRevoked = false
        };
        var revokedToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        };

        await _repository.AddAsync(validToken);
        await _repository.AddAsync(expiredToken);
        await _repository.AddAsync(revokedToken);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetValidTokensByUserIdAsync(userId);

        // Assert
        Assert.Single(result);
        Assert.Equal("valid-token", result.First().Token);
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_RevokesAllUserTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token1 = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "token-1",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        var token2 = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = TokenType.Refresh,
            Token = "token-2",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await _repository.AddAsync(token1);
        await _repository.AddAsync(token2);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.RevokeAllUserTokensAsync(userId);

        // Assert
        var tokens = await _repository.GetByUserIdAsync(userId);
        Assert.All(tokens, t => Assert.True(t.IsRevoked));
    }

    [Fact]
    public async Task DeleteExpiredTokensAsync_DeletesOnlyExpiredTokens()
    {
        // Arrange
        var validToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        var expiredToken = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        await _repository.AddAsync(validToken);
        await _repository.AddAsync(expiredToken);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteExpiredTokensAsync();

        // Assert
        var remainingToken = await _repository.GetByTokenAsync("valid-token");
        Assert.NotNull(remainingToken);
        var deletedToken = await _repository.GetByTokenAsync("expired-token");
        Assert.Null(deletedToken);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsToken()
    {
        // Arrange
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(token);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(token.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(token.Id, result.Id);
    }

    [Fact]
    public async Task AddAsync_WithValidToken_AddsToken()
    {
        // Arrange
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "new-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(token);
        await _repository.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        var retrievedToken = await _repository.GetByTokenAsync("new-token");
        Assert.NotNull(retrievedToken);
    }

    [Fact]
    public async Task UpdateAsync_WithValidToken_UpdatesToken()
    {
        // Arrange
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        await _repository.AddAsync(token);
        await _repository.SaveChangesAsync();

        // Act
        token.IsRevoked = true;
        await _repository.UpdateAsync(token);
        await _repository.SaveChangesAsync();

        // Assert
        var retrievedToken = await _repository.GetByTokenAsync("test-token");
        Assert.NotNull(retrievedToken);
        Assert.True(retrievedToken.IsRevoked);
    }

    [Fact]
    public async Task DeleteAsync_WithValidToken_DeletesToken()
    {
        // Arrange
        var token = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        await _repository.AddAsync(token);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(token);
        await _repository.SaveChangesAsync();

        // Assert
        var retrievedToken = await _repository.GetByTokenAsync("test-token");
        Assert.Null(retrievedToken);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllTokens()
    {
        // Arrange
        var token1 = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "token-1",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };
        var token2 = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Type = TokenType.Refresh,
            Token = "token-2",
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(token1);
        await _repository.AddAsync(token2);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.True(result.Count() >= 2);
    }
}
