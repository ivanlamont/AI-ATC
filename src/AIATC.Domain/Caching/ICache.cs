namespace AIATC.Domain.Caching;

/// <summary>
/// Generic cache interface for abstraction over cache implementations
/// </summary>
public interface ICache
{
    /// <summary>
    /// Gets value from cache
    /// </summary>
    Task<T?> GetAsync<T>(string key);

    /// <summary>
    /// Sets value in cache with optional expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// Removes value from cache
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all keys matching pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Checks if key exists
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets or creates value
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
}
