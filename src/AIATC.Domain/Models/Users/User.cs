using System;
using System.Collections.Generic;

namespace AIATC.Domain.Models.Users;

/// <summary>
/// Represents a user account in the system
/// </summary>
public class User
{
    /// <summary>
    /// Unique user identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Username (unique)
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email address (unique)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Avatar URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User roles
    /// </summary>
    public List<UserRole> Roles { get; set; } = new();

    /// <summary>
    /// OAuth provider (e.g., "google", "github", "local")
    /// </summary>
    public string? OAuthProvider { get; set; }

    /// <summary>
    /// OAuth provider user ID
    /// </summary>
    public string? OAuthProviderId { get; set; }

    /// <summary>
    /// Account creation date (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last login date (UTC)
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Whether the account is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether the email is verified
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// User statistics
    /// </summary>
    public UserStatistics Statistics { get; set; } = new();

    /// <summary>
    /// User preferences
    /// </summary>
    public UserPreferences Preferences { get; set; } = new();

    /// <summary>
    /// Checks if user has a specific role
    /// </summary>
    public bool HasRole(UserRole role)
    {
        return Roles.Contains(role);
    }

    /// <summary>
    /// Checks if user has any of the specified roles
    /// </summary>
    public bool HasAnyRole(params UserRole[] roles)
    {
        foreach (var role in roles)
        {
            if (Roles.Contains(role))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if user is an administrator
    /// </summary>
    public bool IsAdmin => HasRole(UserRole.Administrator);

    /// <summary>
    /// Checks if user is a moderator
    /// </summary>
    public bool IsModerator => HasRole(UserRole.Moderator);

    /// <summary>
    /// Updates last login timestamp
    /// </summary>
    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}

/// <summary>
/// User roles in the system
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Regular user with basic access
    /// </summary>
    User,

    /// <summary>
    /// Premium user with additional features
    /// </summary>
    Premium,

    /// <summary>
    /// Moderator with content management permissions
    /// </summary>
    Moderator,

    /// <summary>
    /// Administrator with full system access
    /// </summary>
    Administrator,

    /// <summary>
    /// Observer with read-only dashboard access
    /// </summary>
    Observer
}

/// <summary>
/// User statistics and achievements
/// </summary>
public class UserStatistics
{
    /// <summary>
    /// Total scenarios completed
    /// </summary>
    public int ScenariosCompleted { get; set; }

    /// <summary>
    /// Total aircraft landed
    /// </summary>
    public int AircraftLanded { get; set; }

    /// <summary>
    /// Total playtime in seconds
    /// </summary>
    public int TotalPlaytimeSeconds { get; set; }

    /// <summary>
    /// Highest score achieved
    /// </summary>
    public int HighestScore { get; set; }

    /// <summary>
    /// Current skill rating (ELO-style)
    /// </summary>
    public int SkillRating { get; set; } = 1000;

    /// <summary>
    /// Total separation violations
    /// </summary>
    public int TotalViolations { get; set; }

    /// <summary>
    /// Perfect scenarios (zero violations)
    /// </summary>
    public int PerfectScenarios { get; set; }

    /// <summary>
    /// Current win streak
    /// </summary>
    public int CurrentStreak { get; set; }

    /// <summary>
    /// Best win streak
    /// </summary>
    public int BestStreak { get; set; }

    /// <summary>
    /// Gets average score
    /// </summary>
    public float GetAverageScore()
    {
        return ScenariosCompleted > 0 ? (float)HighestScore / ScenariosCompleted : 0;
    }

    /// <summary>
    /// Gets success rate (scenarios without violations)
    /// </summary>
    public float GetSuccessRate()
    {
        return ScenariosCompleted > 0 ? (float)PerfectScenarios / ScenariosCompleted : 0;
    }
}

/// <summary>
/// User preferences and settings
/// </summary>
public class UserPreferences
{
    /// <summary>
    /// Preferred theme (light, dark, auto)
    /// </summary>
    public string Theme { get; set; } = "dark";

    /// <summary>
    /// Whether to enable voice commands
    /// </summary>
    public bool VoiceCommandsEnabled { get; set; } = true;

    /// <summary>
    /// Whether to enable TTS readbacks
    /// </summary>
    public bool TextToSpeechEnabled { get; set; } = true;

    /// <summary>
    /// Master volume (0-100)
    /// </summary>
    public int MasterVolume { get; set; } = 80;

    /// <summary>
    /// Preferred difficulty for quick play
    /// </summary>
    public string PreferredDifficulty { get; set; } = "Medium";

    /// <summary>
    /// Whether to show tutorials
    /// </summary>
    public bool ShowTutorials { get; set; } = true;

    /// <summary>
    /// Preferred airport (ICAO code)
    /// </summary>
    public string? PreferredAirport { get; set; }

    /// <summary>
    /// Whether to share statistics publicly
    /// </summary>
    public bool PublicStatistics { get; set; } = true;

    /// <summary>
    /// Email notification preferences
    /// </summary>
    public bool EmailNotifications { get; set; } = true;
}
