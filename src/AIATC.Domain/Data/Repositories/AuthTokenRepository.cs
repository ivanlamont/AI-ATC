using AIATC.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AIATC.Domain.Data.Repositories;

/// <summary>
/// Repository implementation for AuthToken entity
/// </summary>
public class AuthTokenRepository : Repository<AuthToken>, IAuthTokenRepository
{
    public AuthTokenRepository(AircraftControlDbContext context) : base(context)
    {
    }

    public async Task<AuthToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .FirstOrDefaultAsync(t => t.Token == token);
    }

    public async Task<IEnumerable<AuthToken>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<AuthToken>> GetValidTokensByUserIdAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(t => t.UserId == userId &&
                       !t.IsRevoked &&
                       t.ExpiresAt > now)
            .ToListAsync();
    }

    public async Task RevokeAllUserTokensAsync(Guid userId)
    {
        var tokens = await _dbSet
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
        {
            token.Revoke();
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var now = DateTime.UtcNow;
        var expiredTokens = await _dbSet
            .Where(t => t.ExpiresAt <= now)
            .ToListAsync();

        _dbSet.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}
