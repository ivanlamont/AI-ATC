using AIATC.Domain.Data;
using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AIATC.Domain.Tests.Data;

/// <summary>
/// Tests for UserRepository
/// </summary>
public class UserRepositoryTests
{
    private readonly AircraftControlDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AircraftControlDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new AircraftControlDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsUser()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEmailAsync_WithValidEmail_ReturnsUser()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "Test@Example.Com",
            Username = "testuser",
            DisplayName = "Test User",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonexistentEmail_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithValidUsername_ReturnsUser()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_IsCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "TestUser",
            DisplayName = "Test User",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUsernameAsync("testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByUsernameAsync_WithNonexistentUsername_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByOAuthAsync_WithValidProvider_ReturnsUser()
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
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByOAuthAsync("google", "google123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByOAuthAsync_WithInvalidProvider_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByOAuthAsync("google", "invalid");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ReturnsTrue()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("test@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonexistentEmail_ReturnsFalse()
    {
        // Act
        var result = await _repository.EmailExistsAsync("nonexistent@example.com");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task EmailExistsAsync_IsCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "Test@Example.Com",
            Username = "testuser",
            DisplayName = "Test User",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.EmailExistsAsync("test@example.com");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_WithExistingUsername_ReturnsTrue()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.UsernameExistsAsync("testuser");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_WithNonexistentUsername_ReturnsFalse()
    {
        // Act
        var result = await _repository.UsernameExistsAsync("nonexistent");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UsernameExistsAsync_IsCaseInsensitive()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            Username = "TestUser",
            DisplayName = "Test User",
            IsActive = true
        };
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.UsernameExistsAsync("testuser");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetActiveUsersAsync_ReturnOnlyActiveUsers()
    {
        // Arrange
        var activeUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "active@example.com",
            Username = "activeuser",
            DisplayName = "Active User",
            IsActive = true
        };
        var inactiveUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "inactive@example.com",
            Username = "inactiveuser",
            DisplayName = "Inactive User",
            IsActive = false
        };
        await _repository.AddAsync(activeUser);
        await _repository.AddAsync(inactiveUser);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(activeUser.Id, result.First().Id);
    }

    [Fact]
    public async Task GetByRoleAsync_ReturnsUsersWithRole()
    {
        // Arrange
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            Username = "admin",
            DisplayName = "Admin",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.Administrator, UserRole.User }
        };
        var regularUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            Username = "user",
            DisplayName = "User",
            IsActive = true,
            Roles = new List<UserRole> { UserRole.User }
        };
        await _repository.AddAsync(adminUser);
        await _repository.AddAsync(regularUser);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetByRoleAsync(UserRole.Administrator);

        // Assert
        Assert.Single(result);
        Assert.Equal(adminUser.Id, result.First().Id);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllUsers()
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
        await _repository.AddAsync(user1);
        await _repository.AddAsync(user2);
        await _repository.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddAsync_WithValidUser_AddsUser()
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

        // Act
        var result = await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        Assert.NotNull(result);
        var retrievedUser = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(retrievedUser);
    }

    [Fact]
    public async Task UpdateAsync_WithValidUser_UpdatesUser()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        user.DisplayName = "Updated Name";
        await _repository.UpdateAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        var retrievedUser = await _repository.GetByIdAsync(user.Id);
        Assert.NotNull(retrievedUser);
        Assert.Equal("Updated Name", retrievedUser.DisplayName);
    }

    [Fact]
    public async Task DeleteAsync_WithValidUser_DeletesUser()
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
        await _repository.AddAsync(user);
        await _repository.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(user);
        await _repository.SaveChangesAsync();

        // Assert
        var retrievedUser = await _repository.GetByIdAsync(user.Id);
        Assert.Null(retrievedUser);
    }
}
