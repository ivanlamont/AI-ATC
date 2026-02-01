using AIATC.Domain.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for user authentication and authorization
/// </summary>
public class AuthenticationService
{
    private readonly Dictionary<Guid, User> _users = new();
    private readonly Dictionary<string, User> _usersByEmail = new();
    private readonly Dictionary<string, User> _usersByUsername = new();
    private readonly Dictionary<string, AuthToken> _tokens = new();
    private readonly JwtTokenService _jwtService;

    /// <summary>
    /// Event raised when a user logs in
    /// </summary>
    public event EventHandler<UserAuthenticatedEventArgs>? UserAuthenticated;

    /// <summary>
    /// Event raised when a user logs out
    /// </summary>
    public event EventHandler<UserLoggedOutEventArgs>? UserLoggedOut;

    public AuthenticationService(JwtTokenService jwtService)
    {
        _jwtService = jwtService;
    }

    /// <summary>
    /// Registers a new user with OAuth provider
    /// </summary>
    public async Task<User> RegisterOAuthUserAsync(
        string email,
        string username,
        string displayName,
        string oauthProvider,
        string oauthProviderId,
        string? avatarUrl = null)
    {
        // Check if user already exists
        if (_usersByEmail.ContainsKey(email))
        {
            throw new InvalidOperationException($"User with email {email} already exists");
        }

        if (_usersByUsername.ContainsKey(username))
        {
            throw new InvalidOperationException($"Username {username} is already taken");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Username = username,
            DisplayName = displayName,
            OAuthProvider = oauthProvider,
            OAuthProviderId = oauthProviderId,
            AvatarUrl = avatarUrl,
            EmailVerified = true, // OAuth providers verify email
            Roles = new List<UserRole> { UserRole.User },
            CreatedAt = DateTime.UtcNow
        };

        _users[user.Id] = user;
        _usersByEmail[email.ToLower()] = user;
        _usersByUsername[username.ToLower()] = user;

        return await Task.FromResult(user);
    }

    /// <summary>
    /// Authenticates user with OAuth provider
    /// </summary>
    public async Task<AuthTokenResponse> AuthenticateOAuthAsync(
        string email,
        string oauthProvider,
        string oauthProviderId)
    {
        // Find user by OAuth provider info
        var user = _users.Values.FirstOrDefault(u =>
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
            u.OAuthProvider == oauthProvider &&
            u.OAuthProviderId == oauthProviderId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found or OAuth credentials invalid");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        user.RecordLogin();

        // Generate tokens
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.Id);

        // Store refresh token
        var tokenEntity = new AuthToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Type = TokenType.Refresh,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow
        };

        _tokens[refreshToken] = tokenEntity;

        UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs { User = user });

        return await Task.FromResult(new AuthTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600, // 1 hour
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = user.Roles
            }
        });
    }

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    public async Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        if (!_tokens.TryGetValue(refreshToken, out var tokenEntity))
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (!tokenEntity.IsValid)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked");
        }

        if (!_users.TryGetValue(tokenEntity.UserId, out var user))
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Generate new access token
        var accessToken = _jwtService.GenerateAccessToken(user);

        return await Task.FromResult(new AuthTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenType = "Bearer",
            ExpiresIn = 3600,
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                AvatarUrl = user.AvatarUrl,
                Roles = user.Roles
            }
        });
    }

    /// <summary>
    /// Logs out user by revoking their refresh token
    /// </summary>
    public async Task LogoutAsync(string refreshToken)
    {
        if (_tokens.TryGetValue(refreshToken, out var tokenEntity))
        {
            tokenEntity.Revoke();

            if (_users.TryGetValue(tokenEntity.UserId, out var user))
            {
                UserLoggedOut?.Invoke(this, new UserLoggedOutEventArgs { User = user });
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets user by ID
    /// </summary>
    public User? GetUser(Guid userId)
    {
        _users.TryGetValue(userId, out var user);
        return user;
    }

    /// <summary>
    /// Gets user by email
    /// </summary>
    public User? GetUserByEmail(string email)
    {
        _usersByEmail.TryGetValue(email.ToLower(), out var user);
        return user;
    }

    /// <summary>
    /// Gets user by username
    /// </summary>
    public User? GetUserByUsername(string username)
    {
        _usersByUsername.TryGetValue(username.ToLower(), out var user);
        return user;
    }

    /// <summary>
    /// Updates user roles (admin only)
    /// </summary>
    public async Task UpdateUserRolesAsync(Guid userId, List<UserRole> roles)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException("User not found");
        }

        user.Roles = roles;
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deactivates user account
    /// </summary>
    public async Task DeactivateUserAsync(Guid userId)
    {
        if (!_users.TryGetValue(userId, out var user))
        {
            throw new InvalidOperationException("User not found");
        }

        user.IsActive = false;

        // Revoke all refresh tokens
        foreach (var token in _tokens.Values.Where(t => t.UserId == userId))
        {
            token.Revoke();
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    public IEnumerable<User> GetAllUsers()
    {
        return _users.Values;
    }

    /// <summary>
    /// Generates a secure refresh token
    /// </summary>
    private string GenerateRefreshToken(Guid userId)
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        var token = Convert.ToBase64String(randomBytes);
        return $"{userId}_{token}";
    }
}

/// <summary>
/// Event args for user authentication
/// </summary>
public class UserAuthenticatedEventArgs : EventArgs
{
    public User User { get; set; } = null!;
}

/// <summary>
/// Event args for user logout
/// </summary>
public class UserLoggedOutEventArgs : EventArgs
{
    public User User { get; set; } = null!;
}
