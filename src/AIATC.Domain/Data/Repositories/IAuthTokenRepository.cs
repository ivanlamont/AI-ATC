using AIATC.Domain.Models.Users;

namespace AIATC.Domain.Data.Repositories;

/// <summary>
/// Repository interface for AuthToken entity
/// </summary>
public interface IAuthTokenRepository : IRepository<AuthToken>
{
    /// <summary>
    /// Gets token by token string
    /// </summary>
    Task<AuthToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Gets all tokens for a user
    /// </summary>
    Task<IEnumerable<AuthToken>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Gets valid (not expired, not revoked) tokens for a user
    /// </summary>
    Task<IEnumerable<AuthToken>> GetValidTokensByUserIdAsync(Guid userId);

    /// <summary>
    /// Revokes all tokens for a user
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId);

    /// <summary>
    /// Deletes expired tokens (cleanup)
    /// </summary>
    Task DeleteExpiredTokensAsync();
}
