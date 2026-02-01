using AIATC.Domain.Data.Repositories;
using AIATC.Domain.Models.Users;
using System.Security.Cryptography;

namespace AIATC.Domain.Services;

/// <summary>
/// Database-backed authentication service using Entity Framework Core
/// </summary>
public class DatabaseAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthTokenRepository _tokenRepository;
    private readonly JwtTokenService _jwtService;

    /// <summary>
    /// Event raised when a user logs in
    /// </summary>
    public event EventHandler<UserAuthenticatedEventArgs>? UserAuthenticated;

    /// <summary>
    /// Event raised when a user logs out
    /// </summary>
    public event EventHandler<UserLoggedOutEventArgs>? UserLoggedOut;

    public DatabaseAuthenticationService(
        IUserRepository userRepository,
        IAuthTokenRepository tokenRepository,
        JwtTokenService jwtService)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
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
        if (await _userRepository.EmailExistsAsync(email))
        {
            throw new InvalidOperationException($"User with email {email} already exists");
        }

        if (await _userRepository.UsernameExistsAsync(username))
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

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
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
        var user = await _userRepository.GetByOAuthAsync(oauthProvider, oauthProviderId);

        if (user == null || !user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("User not found or OAuth credentials invalid");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is inactive");
        }

        user.RecordLogin();
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

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

        await _tokenRepository.AddAsync(tokenEntity);
        await _tokenRepository.SaveChangesAsync();

        UserAuthenticated?.Invoke(this, new UserAuthenticatedEventArgs { User = user });

        return new AuthTokenResponse
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
        };
    }

    /// <summary>
    /// Refreshes access token using refresh token
    /// </summary>
    public async Task<AuthTokenResponse> RefreshTokenAsync(string refreshToken)
    {
        var tokenEntity = await _tokenRepository.GetByTokenAsync(refreshToken);

        if (tokenEntity == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        if (!tokenEntity.IsValid)
        {
            throw new UnauthorizedAccessException("Refresh token is expired or revoked");
        }

        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);

        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Generate new access token
        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthTokenResponse
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
        };
    }

    /// <summary>
    /// Logs out user by revoking their refresh token
    /// </summary>
    public async Task LogoutAsync(string refreshToken)
    {
        var tokenEntity = await _tokenRepository.GetByTokenAsync(refreshToken);

        if (tokenEntity != null)
        {
            tokenEntity.Revoke();
            await _tokenRepository.UpdateAsync(tokenEntity);
            await _tokenRepository.SaveChangesAsync();

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
            if (user != null)
            {
                UserLoggedOut?.Invoke(this, new UserLoggedOutEventArgs { User = user });
            }
        }
    }

    /// <summary>
    /// Gets user by ID
    /// </summary>
    public async Task<User?> GetUserAsync(Guid userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    /// <summary>
    /// Gets user by email
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userRepository.GetByEmailAsync(email);
    }

    /// <summary>
    /// Gets user by username
    /// </summary>
    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _userRepository.GetByUsernameAsync(username);
    }

    /// <summary>
    /// Updates user roles (admin only)
    /// </summary>
    public async Task UpdateUserRolesAsync(Guid userId, List<UserRole> roles)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Roles = roles;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Deactivates user account
    /// </summary>
    public async Task DeactivateUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);

        // Revoke all refresh tokens
        await _tokenRepository.RevokeAllUserTokensAsync(userId);
        await _userRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Gets all users
    /// </summary>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    /// <summary>
    /// Gets active users
    /// </summary>
    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetActiveUsersAsync();
    }

    /// <summary>
    /// Gets users by role
    /// </summary>
    public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role)
    {
        return await _userRepository.GetByRoleAsync(role);
    }

    /// <summary>
    /// Cleanup expired tokens (maintenance task)
    /// </summary>
    public async Task CleanupExpiredTokensAsync()
    {
        await _tokenRepository.DeleteExpiredTokensAsync();
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
