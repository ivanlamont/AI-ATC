namespace AIATC.Domain.Caching;

/// <summary>
/// No-op cache implementation for when caching is disabled
/// </summary>
public class NullCache : ICache
{
    public Task<T?> GetAsync<T>(string key) => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) => Task.CompletedTask;

    public Task RemoveAsync(string key) => Task.CompletedTask;

    public Task RemoveByPatternAsync(string pattern) => Task.CompletedTask;

    public Task<bool> ExistsAsync(string key) => Task.FromResult(false);

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        return await factory();
    }
}
