using AIATC.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;

namespace AIATC.Domain.Data.Configurations;

/// <summary>
/// Entity configuration for User model
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        // Primary key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasColumnName("id")
            .IsRequired();

        // Basic properties
        builder.Property(u => u.Username)
            .HasColumnName("username")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(500);

        // OAuth properties
        builder.Property(u => u.OAuthProvider)
            .HasColumnName("oauth_provider")
            .HasMaxLength(50);

        builder.Property(u => u.OAuthProviderId)
            .HasColumnName("oauth_provider_id")
            .HasMaxLength(255);

        // Status properties
        builder.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(u => u.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired()
            .HasDefaultValue(false);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.LastLoginAt)
            .HasColumnName("last_login_at");

        // Store roles as JSON array
        builder.Property(u => u.Roles)
            .HasColumnName("roles")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<UserRole>>(v, (JsonSerializerOptions?)null) ?? new List<UserRole>())
            .HasColumnType("jsonb")
            .IsRequired();

        // Store statistics as JSON
        builder.OwnsOne(u => u.Statistics, stats =>
        {
            stats.ToJson("statistics");
            stats.Property(s => s.ScenariosCompleted).HasJsonPropertyName("scenarios_completed");
            stats.Property(s => s.AircraftLanded).HasJsonPropertyName("aircraft_landed");
            stats.Property(s => s.TotalPlaytimeSeconds).HasJsonPropertyName("total_playtime_seconds");
            stats.Property(s => s.HighestScore).HasJsonPropertyName("highest_score");
            stats.Property(s => s.SkillRating).HasJsonPropertyName("skill_rating");
            stats.Property(s => s.TotalViolations).HasJsonPropertyName("total_violations");
            stats.Property(s => s.PerfectScenarios).HasJsonPropertyName("perfect_scenarios");
            stats.Property(s => s.CurrentStreak).HasJsonPropertyName("current_streak");
            stats.Property(s => s.BestStreak).HasJsonPropertyName("best_streak");
        });

        // Store preferences as JSON
        builder.OwnsOne(u => u.Preferences, prefs =>
        {
            prefs.ToJson("preferences");
            prefs.Property(p => p.Theme).HasJsonPropertyName("theme");
            prefs.Property(p => p.VoiceCommandsEnabled).HasJsonPropertyName("voice_commands_enabled");
            prefs.Property(p => p.TextToSpeechEnabled).HasJsonPropertyName("text_to_speech_enabled");
            prefs.Property(p => p.MasterVolume).HasJsonPropertyName("master_volume");
            prefs.Property(p => p.PreferredDifficulty).HasJsonPropertyName("preferred_difficulty");
            prefs.Property(p => p.ShowTutorials).HasJsonPropertyName("show_tutorials");
            prefs.Property(p => p.PreferredAirport).HasJsonPropertyName("preferred_airport");
            prefs.Property(p => p.PublicStatistics).HasJsonPropertyName("public_statistics");
            prefs.Property(p => p.EmailNotifications).HasJsonPropertyName("email_notifications");
        });

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.HasIndex(u => u.Username)
            .IsUnique()
            .HasDatabaseName("ix_users_username");

        builder.HasIndex(u => new { u.OAuthProvider, u.OAuthProviderId })
            .HasDatabaseName("ix_users_oauth");

        builder.HasIndex(u => u.IsActive)
            .HasDatabaseName("ix_users_is_active");
    }
}
