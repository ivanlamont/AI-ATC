using AIATC.Domain.Caching;
using AIATC.Domain.Models.Users;

namespace AIATC.Domain.Data.Repositories;

/// <summary>
/// User repository with caching layer
/// </summary>
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _inner;
    private readonly ICache _cache;
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);

    public CachedUserRepository(IUserRepository inner, ICache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var key = CacheKeys.User(id);
        return await _cache.GetOrSetAsync(key, () => _inner.GetByIdAsync(id), _cacheDuration);
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<IEnumerable<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
        return await _inner.FindAsync(predicate);
    }

    public async Task<User?> FirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
        return await _inner.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
    {
        return await _inner.AnyAsync(predicate);
    }

    public async Task<User> AddAsync(User entity)
    {
        var result = await _inner.AddAsync(entity);
        await InvalidateCacheAsync(entity.Id);
        return result;
    }

    public async Task UpdateAsync(User entity)
    {
        await _inner.UpdateAsync(entity);
        await InvalidateCacheAsync(entity.Id);
    }

    public async Task DeleteAsync(User entity)
    {
        await _inner.DeleteAsync(entity);
        await InvalidateCacheAsync(entity.Id);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _inner.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        var key = CacheKeys.UserByEmail(email);
        return await _cache.GetOrSetAsync(key, () => _inner.GetByEmailAsync(email), _cacheDuration);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var key = CacheKeys.UserByUsername(username);
        return await _cache.GetOrSetAsync(key, () => _inner.GetByUsernameAsync(username), _cacheDuration);
    }

    public async Task<User?> GetByOAuthAsync(string provider, string providerId)
    {
        return await _inner.GetByOAuthAsync(provider, providerId);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _inner.EmailExistsAsync(email);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _inner.UsernameExistsAsync(username);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _inner.GetActiveUsersAsync();
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role)
    {
        return await _inner.GetByRoleAsync(role);
    }

    private async Task InvalidateCacheAsync(Guid userId)
    {
        await _cache.RemoveAsync(CacheKeys.User(userId));
        await _cache.RemoveByPatternAsync(CacheKeys.UserActivityPattern);
    }
}
