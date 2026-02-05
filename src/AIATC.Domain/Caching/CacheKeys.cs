namespace AIATC.Domain.Caching;

/// <summary>
/// Constants for cache keys
/// </summary>
public static class CacheKeys
{
    private const string Prefix = "aiatc:";

    // User keys
    public static string User(Guid userId) => $"{Prefix}user:{userId}";
    public static string UserByEmail(string email) => $"{Prefix}user:email:{email.ToLower()}";
    public static string UserByUsername(string username) => $"{Prefix}user:username:{username.ToLower()}";
    public const string UserPattern = $"{Prefix}user:*";

    // Leaderboard keys
    public static string Leaderboard(string type, string timeFrame, int page) =>
        $"{Prefix}leaderboard:{type}:{timeFrame}:p{page}";
    public const string LeaderboardPattern = $"{Prefix}leaderboard:*";

    // System metrics
    public const string SystemMetrics = $"{Prefix}metrics:system";

    // Active tokens count
    public const string ActiveTokenCount = $"{Prefix}metrics:active_tokens";

    // User activity keys
    public static string UserActivity(int page = 1) => $"{Prefix}activity:users:p{page}";
    public const string UserActivityPattern = $"{Prefix}activity:*";

    // Top performers
    public const string TopPerformers = $"{Prefix}topperformers";

    // Configuration
    public const string CacheDuration = "cache:duration";
}
