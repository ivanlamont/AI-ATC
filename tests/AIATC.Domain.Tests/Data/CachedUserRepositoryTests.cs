using AIATC.Domain.Caching;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using Moq;
using Xunit;

namespace AIATC.Domain.Tests.Data;

/// <summary>
/// Tests for CachedUserRepository decorator pattern
/// </summary>
public class CachedUserRepositoryTests
{
    private readonly Mock<IUserRepository> _innerRepositoryMock;
    private readonly Mock<ICache> _cacheMock;
    private readonly CachedUserRepository _repository;

    public CachedUserRepositoryTests()
    {
        _innerRepositoryMock = new Mock<IUserRepository>();
        _cacheMock = new Mock<ICache>();
        _repository = new CachedUserRepository(_innerRepositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_CachesResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "test@example.com", Username = "testuser" };

        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(
                (key, factory, duration) => factory());

        _innerRepositoryMock
            .Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Id);
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_CachesResult()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(
                (key, factory, duration) => factory());

        _innerRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync(user);

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()),
            Times.Once);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_CachesResult()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _cacheMock
            .Setup(c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()))
            .Returns<string, Func<Task<User?>>, TimeSpan>(
                (key, factory, duration) => factory());

        _innerRepositoryMock
            .Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("testuser", result.Username);
    }

    [Fact]
    public async Task GetByOAuthAsync_BypassesCache()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", OAuthProvider = "google" };

        _innerRepositoryMock
            .Setup(r => r.GetByOAuthAsync("google", "google123"))
            .ReturnsAsync(user);

        // Act
        var result = await _repository.GetByOAuthAsync("google", "google123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("google", result.OAuthProvider);
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task AddAsync_InvalidatesCacheForUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _innerRepositoryMock
            .Setup(r => r.AddAsync(user))
            .ReturnsAsync(user);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.AddAsync(user);

        // Assert
        Assert.NotNull(result);
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidatesCacheForUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _innerRepositoryMock
            .Setup(r => r.UpdateAsync(user))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.UpdateAsync(user);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_InvalidatesCacheForUser()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Username = "testuser" };

        _innerRepositoryMock
            .Setup(r => r.DeleteAsync(user))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveByPatternAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        await _repository.DeleteAsync(user);

        // Assert
        _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPatternAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_BypassesCache()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com", Username = "user1" },
            new User { Id = Guid.NewGuid(), Email = "user2@example.com", Username = "user2" }
        };

        _innerRepositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
        _cacheMock.Verify(
            c => c.GetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<User?>>>(),
                It.IsAny<TimeSpan>()),
            Times.Never);
    }

    [Fact]
    public async Task FindAsync_BypassesCache()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com", Username = "user1", IsActive = true }
        };

        _innerRepositoryMock
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(users);

        // Act
        var result = await _repository.FindAsync(u => u.IsActive);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task FirstOrDefaultAsync_BypassesCache()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", IsActive = true };

        _innerRepositoryMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(user);

        // Act
        var result = await _repository.FirstOrDefaultAsync(u => u.IsActive);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AnyAsync_BypassesCache()
    {
        // Arrange
        _innerRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<User, bool>>>()))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.AnyAsync(u => u.IsActive);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetActiveUsersAsync_BypassesCache()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "user1@example.com", IsActive = true }
        };

        _innerRepositoryMock
            .Setup(r => r.GetActiveUsersAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByRoleAsync_BypassesCache()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = Guid.NewGuid(), Email = "admin@example.com", Roles = new List<UserRole> { UserRole.Administrator } }
        };

        _innerRepositoryMock
            .Setup(r => r.GetByRoleAsync(UserRole.Administrator))
            .ReturnsAsync(users);

        // Act
        var result = await _repository.GetByRoleAsync(UserRole.Administrator);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task EmailExistsAsync_BypassesCache()
    {
        // Arrange
        _innerRepositoryMock
            .Setup(r => r.EmailExistsAsync("test@example.com"))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.EmailExistsAsync("test@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_BypassesCache()
    {
        // Arrange
        _innerRepositoryMock
            .Setup(r => r.UsernameExistsAsync("testuser"))
            .ReturnsAsync(true);

        // Act
        var result = await _repository.UsernameExistsAsync("testuser");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SaveChangesAsync_CallsInnerRepository()
    {
        // Arrange
        _innerRepositoryMock
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _repository.SaveChangesAsync();

        // Assert
        Assert.Equal(1, result);
        _innerRepositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
