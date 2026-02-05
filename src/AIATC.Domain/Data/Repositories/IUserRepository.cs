using AIATC.Domain.Models.Users;

namespace AIATC.Domain.Data.Repositories;

/// <summary>
/// Repository interface for User entity
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Gets user by email (case-insensitive)
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Gets user by username (case-insensitive)
    /// </summary>
    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Gets user by OAuth provider and provider ID
    /// </summary>
    Task<User?> GetByOAuthAsync(string provider, string providerId);

    /// <summary>
    /// Checks if email is already registered
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Checks if username is already taken
    /// </summary>
    Task<bool> UsernameExistsAsync(string username);

    /// <summary>
    /// Gets active users
    /// </summary>
    Task<IEnumerable<User>> GetActiveUsersAsync();

    /// <summary>
    /// Gets users by role
    /// </summary>
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role);
}
