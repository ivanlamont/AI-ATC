using System;

namespace AIATC.Domain.Models.Users;

/// <summary>
/// Represents an authentication token (JWT or refresh token)
/// </summary>
public class AuthToken
{
    /// <summary>
    /// Token identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User ID this token belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Token type (access, refresh)
    /// </summary>
    public TokenType Type { get; set; }

    /// <summary>
    /// The actual token string
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration date (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Token creation date (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the token has been revoked
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// When the token was revoked (UTC)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// IP address where token was created
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent where token was created
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Checks if the token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Checks if the token is valid (not expired and not revoked)
    /// </summary>
    public bool IsValid => !IsExpired && !IsRevoked;

    /// <summary>
    /// Revokes the token
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Types of authentication tokens
/// </summary>
public enum TokenType
{
    /// <summary>
    /// Short-lived access token (JWT)
    /// </summary>
    Access,

    /// <summary>
    /// Long-lived refresh token
    /// </summary>
    Refresh
}

/// <summary>
/// Response containing authentication tokens
/// </summary>
public class AuthTokenResponse
{
    /// <summary>
    /// Access token (JWT)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Access token expiration in seconds
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// User information
    /// </summary>
    public UserInfo? User { get; set; }
}

/// <summary>
/// Basic user information for token response
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public List<UserRole> Roles { get; set; } = new();
}
