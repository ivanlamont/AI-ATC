using AIATC.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AIATC.Domain.Data.Configurations;

/// <summary>
/// Entity configuration for AuthToken model
/// </summary>
public class AuthTokenConfiguration : IEntityTypeConfiguration<AuthToken>
{
    public void Configure(EntityTypeBuilder<AuthToken> builder)
    {
        builder.ToTable("auth_tokens");

        // Primary key
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        // Foreign key to User
        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Token properties
        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // Revocation properties
        builder.Property(t => t.IsRevoked)
            .HasColumnName("is_revoked")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(t => t.RevokedAt)
            .HasColumnName("revoked_at");

        // Metadata properties
        builder.Property(t => t.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(50);

        builder.Property(t => t.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("ix_auth_tokens_token");

        builder.HasIndex(t => t.UserId)
            .HasDatabaseName("ix_auth_tokens_user_id");

        builder.HasIndex(t => t.ExpiresAt)
            .HasDatabaseName("ix_auth_tokens_expires_at");

        builder.HasIndex(t => t.IsRevoked)
            .HasDatabaseName("ix_auth_tokens_is_revoked");

        // Relationship with User
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
