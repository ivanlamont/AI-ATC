namespace AIATC.BFF;

/// <summary>
/// Thread-safe in-process cache for Azure Speech tokens.
/// Azure tokens expire after 10 minutes; we refresh after 9.
/// </summary>
public sealed class SpeechTokenCache
{
    private readonly SemaphoreSlim _lock = new(1, 1);
    private string _token = string.Empty;
    private DateTimeOffset _fetchedAt = DateTimeOffset.MinValue;
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(9);

    public bool IsExpired => DateTimeOffset.UtcNow - _fetchedAt >= TokenLifetime;

    public string Token => _token;

    public void Set(string token)
    {
        _token = token;
        _fetchedAt = DateTimeOffset.UtcNow;
    }

    public SemaphoreSlim Lock => _lock;
}
