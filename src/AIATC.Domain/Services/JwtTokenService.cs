using AIATC.Domain.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace AIATC.Domain.Services;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public class JwtTokenService
{
    private readonly JwtConfiguration _configuration;

    public JwtTokenService(JwtConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Generates an access token (JWT) for a user
    /// </summary>
    public string GenerateAccessToken(User user)
    {
        // Create token payload with user claims
        // In production, use proper JWT library (System.IdentityModel.Tokens.Jwt)
        var tokenDescriptor = new
        {
            sub = user.Id.ToString(),
            email = user.Email,
            unique_name = user.Username,
            display_name = user.DisplayName,
            roles = user.Roles.Select(r => r.ToString()).ToList(),
            jti = Guid.NewGuid().ToString(),
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddSeconds(_configuration.AccessTokenExpirationSeconds).ToUnixTimeSeconds(),
            iss = _configuration.Issuer,
            aud = _configuration.Audience
        };

        // For now, return a simplified JWT-like string
        // In production, use proper JWT library with signing
        var token = EncodeToken(tokenDescriptor);
        return token;
    }

    /// <summary>
    /// Validates an access token
    /// </summary>
    public TokenValidationResult ValidateAccessToken(string token)
    {
        try
        {
            var payload = DecodeToken(token);

            // Check expiration
            if (payload.TryGetValue("exp", out var expObj))
            {
                long exp = 0;
                if (expObj is JsonElement expElement && expElement.ValueKind == JsonValueKind.Number)
                {
                    exp = expElement.GetInt64();
                }
                else if (expObj is long expLong)
                {
                    exp = expLong;
                }

                if (exp > 0)
                {
                    var expirationTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                    if (expirationTime <= DateTimeOffset.UtcNow)
                    {
                        return new TokenValidationResult
                        {
                            IsValid = false,
                            ErrorMessage = "Token has expired"
                        };
                    }
                }
            }

            // Extract user ID
            if (payload.TryGetValue("sub", out var subObj))
            {
                string? sub = null;
                if (subObj is JsonElement subElement && subElement.ValueKind == JsonValueKind.String)
                {
                    sub = subElement.GetString();
                }
                else if (subObj is string subStr)
                {
                    sub = subStr;
                }

                if (!string.IsNullOrEmpty(sub))
                {
                    return new TokenValidationResult
                    {
                        IsValid = true,
                        UserId = Guid.Parse(sub),
                        Claims = payload
                    };
                }
            }

            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "Invalid token format"
            };
        }
        catch (Exception ex)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token validation failed: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Extracts user ID from token without full validation
    /// </summary>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var payload = DecodeToken(token);
            if (payload.TryGetValue("sub", out var subObj))
            {
                string? sub = null;
                if (subObj is JsonElement subElement && subElement.ValueKind == JsonValueKind.String)
                {
                    sub = subElement.GetString();
                }
                else if (subObj is string subStr)
                {
                    sub = subStr;
                }

                if (!string.IsNullOrEmpty(sub))
                {
                    return Guid.Parse(sub);
                }
            }
        }
        catch
        {
            // Invalid token
        }

        return null;
    }

    // Simplified encoding (for demonstration)
    // In production, use proper JWT signing with HMACSHA256 or RSA
    private string EncodeToken(object tokenDescriptor)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(tokenDescriptor);
        var bytes = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);
        return $"eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.{base64}.signature";
    }

    private Dictionary<string, object> DecodeToken(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            throw new FormatException("Invalid token format");
        }

        var payload = parts[1];
        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);
        var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
        return dict ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// JWT configuration settings
/// </summary>
public class JwtConfiguration
{
    /// <summary>
    /// Token issuer (your application)
    /// </summary>
    public string Issuer { get; set; } = "AI-ATC";

    /// <summary>
    /// Token audience (your application)
    /// </summary>
    public string Audience { get; set; } = "AI-ATC-Users";

    /// <summary>
    /// Secret key for signing tokens (use strong key in production)
    /// </summary>
    public string SecretKey { get; set; } = "your-256-bit-secret-key-here-change-in-production";

    /// <summary>
    /// Access token expiration in seconds (default: 1 hour)
    /// </summary>
    public int AccessTokenExpirationSeconds { get; set; } = 3600;

    /// <summary>
    /// Refresh token expiration in seconds (default: 30 days)
    /// </summary>
    public int RefreshTokenExpirationSeconds { get; set; } = 2592000;
}

/// <summary>
/// Result of token validation
/// </summary>
public class TokenValidationResult
{
    /// <summary>
    /// Whether the token is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// User ID extracted from token
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Claims extracted from token
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
