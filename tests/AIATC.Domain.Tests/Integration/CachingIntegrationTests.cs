using AIATC.Domain.Caching;
using AIATC.Domain.Data;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using AIATC.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AIATC.Domain.Tests.Integration;

/// <summary>
/// Integration tests for caching layer with services
/// </summary>
public class CachingIntegrationTests : IDisposable
{
    private readonly AircraftControlDbContext _context;
    private readonly UserRepository _userRepository;
    private readonly Mock<ICache> _cacheMock;
    private readonly CachedUserRepository _cachedRepository;
    private readonly DatabaseAuthenticationService _authService;
    private readonly JwtTokenService _jwtService;

    public CachingIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AircraftControlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AircraftControlDbContext(options);
        _userRepository = new UserRepository(_context);
        _cacheMock = new Mock<ICache>();
        _cachedRepository = new CachedUserRepository(_userRepository, _cacheMock.Object);

        var config = new JwtConfiguration
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenExpirationSeconds = 3600
        };
        _jwtService = new JwtTokenService(config);

        var tokenRepository = new AuthTokenRepository(_context);
        _authService = new DatabaseAuthenticationService(
            _cachedRepository,
            tokenRepository,
            _jwtService);
    }

    [Fact]
    public async Task UserAuthentication_CachesUserData()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            OAuthProvider = "google",
            OAuthProviderId = "google123",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Setup cache mock to track calls
        var cacheHits = new Dictionary<string, User>();
        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(async (key, factory, duration) =>
            {
                if (cacheHits.ContainsKey(key))
                {
                    return cacheHits[key];
                }
                var result = await factory();
                if (result != null)
                {
                    cacheHits[key] = result;
                }
                return result;
            });

        // Act - First authentication (should query DB)
        var auth1 = await _authService.AuthenticateOAuthAsync(
            "test@example.com",
            "google",
            "google123");

        // Act - Retrieve user by ID (should use cache)
        var cachedUser = await _cachedRepository.GetByIdAsync(user.Id);

        // Assert - User should be retrieved
        Assert.NotNull(cachedUser);
        Assert.Equal(user.Id, cachedUser.Id);

        // Assert - Cache mock should have been called
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task UserUpdate_InvalidatesCachedData()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act - Update user
        user.DisplayName = "Updated User";
        await _cachedRepository.UpdateAsync(user);

        // Assert - Cache should be invalidated
        _cacheMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Once);

        _cacheMock.Verify(
            c => c.RemoveByPatternAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task UserDeletion_InvalidatesCachedData()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "testuser",
            DisplayName = "Test User",
            IsActive = true
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act - Delete user
        await _cachedRepository.DeleteAsync(user);

        // Assert - Cache should be invalidated
        _cacheMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task MultipleUserOperations_CacheCoherence()
    {
        // Arrange
        var user1 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user1@example.com",
            Username = "user1",
            DisplayName = "User One",
            IsActive = true
        };

        var user2 = new User
        {
            Id = Guid.NewGuid(),
            Email = "user2@example.com",
            Username = "user2",
            DisplayName = "User Two",
            IsActive = true
        };

        await _userRepository.AddAsync(user1);
        await _userRepository.AddAsync(user2);
        await _userRepository.SaveChangesAsync();

        var cacheData = new Dictionary<string, User>();

        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(async (key, factory, duration) =>
            {
                if (cacheData.ContainsKey(key))
                {
                    return cacheData[key];
                }
                var result = await factory();
                if (result != null)
                {
                    cacheData[key] = result;
                }
                return result;
            });

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns<string>(key =>
            {
                cacheData.Remove(key);
                return Task.CompletedTask;
            });

        _cacheMock
            .Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act - Load user1 into cache
        var cached1 = await _cachedRepository.GetByIdAsync(user1.Id);
        Assert.Single(cacheData);

        // Act - Load user2 into cache
        var cached2 = await _cachedRepository.GetByIdAsync(user2.Id);
        Assert.Equal(2, cacheData.Count);

        // Act - Update user1
        user1.DisplayName = "Updated User One";
        await _cachedRepository.UpdateAsync(user1);

        // Assert - Only user1 cache entry should be cleared
        Assert.Single(cacheData); // user2 still cached

        // Act - Load user1 again (should reflect update)
        var updated1 = await _cachedRepository.GetByIdAsync(user1.Id);
        Assert.Equal("Updated User One", updated1!.DisplayName);
    }

    [Fact]
    public async Task ConcurrentUserAccess_CacheThreadSafety()
    {
        // Arrange
        var users = Enumerable.Range(1, 5)
            .Select(i => new User
            {
                Id = Guid.NewGuid(),
                Email = $"user{i}@example.com",
                Username = $"user{i}",
                DisplayName = $"User {i}",
                IsActive = true
            })
            .ToList();

        foreach (var user in users)
        {
            await _userRepository.AddAsync(user);
        }
        await _userRepository.SaveChangesAsync();

        var cacheData = new Dictionary<string, User>();

        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(async (key, factory, duration) =>
            {
                if (cacheData.ContainsKey(key))
                {
                    return cacheData[key];
                }
                var result = await factory();
                if (result != null)
                {
                    cacheData[key] = result;
                }
                return result;
            });

        // Act - Concurrent access to multiple users
        var tasks = users.Select(u => _cachedRepository.GetByIdAsync(u.Id)).ToList();
        var results = await Task.WhenAll(tasks);

        // Assert - All users should be retrieved
        Assert.Equal(5, results.Length);
        Assert.All(results, r => Assert.NotNull(r));

        // Assert - All should be cached
        Assert.Equal(5, cacheData.Count);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
