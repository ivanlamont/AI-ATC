using Microsoft.EntityFrameworkCore;
using AIATC.Domain.Entities;

namespace AIATC.Domain.Data;

public class ATCDbContext : DbContext
{
    public ATCDbContext(DbContextOptions<ATCDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users => Set<User>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<SessionCommand> SessionCommands => Set<SessionCommand>();
    public DbSet<SessionEvent> SessionEvents => Set<SessionEvent>();
    public DbSet<Score> Scores => Set<Score>();
    public DbSet<Scenario> Scenarios => Set<Scenario>();
    public DbSet<SavedScenario> SavedScenarios => Set<SavedScenario>();
    public DbSet<Weather> WeatherRecords => Set<Weather>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OAuthProvider).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OAuthSubjectId).HasMaxLength(255).IsRequired();
            entity.Property(e => e.DisplayName).HasMaxLength(100);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired().HasDefaultValue("user");
            entity.Property(e => e.IsGuest).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => new { e.OAuthProvider, e.OAuthSubjectId }).IsUnique();
            entity.HasIndex(e => e.Role);
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("active");
            entity.Property(e => e.TimeAcceleration).HasDefaultValue(1.0f);
            entity.Property(e => e.AircraftControlled).HasDefaultValue(0);
            entity.Property(e => e.CommandsIssued).HasDefaultValue(0);
            entity.Property(e => e.SeparationViolations).HasDefaultValue(0);
            entity.Property(e => e.SuccessfulLandings).HasDefaultValue(0);
            entity.Property(e => e.SuccessfulHandoffs).HasDefaultValue(0);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Scenario)
                .WithMany(s => s.Sessions)
                .HasForeignKey(e => e.ScenarioId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ScenarioId);
            entity.HasIndex(e => e.StartedAt).IsDescending();
            entity.HasIndex(e => e.Status).HasFilter("status = 'active'");
        });

        // SessionCommand configuration
        modelBuilder.Entity<SessionCommand>(entity =>
        {
            entity.ToTable("session_commands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AircraftId).HasMaxLength(20).IsRequired();
            entity.Property(e => e.CommandType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CommandText).IsRequired();
            entity.Property(e => e.IssuedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Session)
                .WithMany(s => s.Commands)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SessionId, e.SimulationTime });
        });

        // SessionEvent configuration
        modelBuilder.Entity<SessionEvent>(entity =>
        {
            entity.ToTable("session_events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.Session)
                .WithMany(s => s.Events)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SessionId, e.SimulationTime });
            entity.HasIndex(e => e.EventType);
        });

        // Score configuration
        modelBuilder.Entity<Score>(entity =>
        {
            entity.ToTable("scores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeAcceleration).HasDefaultValue(1.0f);
            entity.Property(e => e.AchievedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Scores)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Session)
                .WithOne()
                .HasForeignKey<Score>(e => e.SessionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Scenario)
                .WithMany(s => s.Scores)
                .HasForeignKey(e => e.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ScenarioId);
            entity.HasIndex(e => new { e.ScenarioId, e.AdjustedScore, e.AchievedAt }).IsDescending();
            entity.HasIndex(e => e.AchievedAt).IsDescending();
        });

        // Scenario configuration
        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.ToTable("scenarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.AirportCode).HasMaxLength(4).IsRequired();
            entity.Property(e => e.ScenarioType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsPublic).HasDefaultValue(false);
            entity.Property(e => e.PlayCount).HasDefaultValue(0);

            entity.HasOne(e => e.Creator)
                .WithMany(u => u.CreatedScenarios)
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.AirportCode);
            entity.HasIndex(e => e.DifficultyLevel);
            entity.HasIndex(e => e.IsPublic).HasFilter("is_public = true");
        });

        // SavedScenario configuration
        modelBuilder.Entity<SavedScenario>(entity =>
        {
            entity.ToTable("saved_scenarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SaveName).HasMaxLength(200);
            entity.Property(e => e.SavedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.SavedScenarios)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Scenario)
                .WithMany(s => s.SavedProgress)
                .HasForeignKey(e => e.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.ScenarioId, e.SaveName }).IsUnique();
        });

        // Weather configuration
        modelBuilder.Entity<Weather>(entity =>
        {
            entity.ToTable("weather");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VisibilitySm).HasPrecision(4, 2);
            entity.Property(e => e.AltimeterInHg).HasPrecision(5, 2);
            entity.Property(e => e.Source).HasMaxLength(20);
            entity.Property(e => e.FetchedAt).HasDefaultValueSql("NOW()");

            // Airport relationship removed - will be added back when consolidating with ARINC models
            entity.HasIndex(e => e.ValidFrom).IsDescending();
            entity.HasIndex(e => new { e.AirportId, e.ValidFrom }).IsDescending();
        });

        // Achievement configuration
        modelBuilder.Entity<Achievement>(entity =>
        {
            entity.ToTable("achievements");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.IconUrl).HasMaxLength(500);
            entity.Property(e => e.Tier).HasMaxLength(20);
            entity.Property(e => e.Points).HasDefaultValue(0);

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // UserAchievement configuration
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.ToTable("user_achievements");
            entity.HasKey(e => new { e.UserId, e.AchievementId });
            entity.Property(e => e.EarnedAt).HasDefaultValueSql("NOW()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.Achievements)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Achievement)
                .WithMany(a => a.UserAchievements)
                .HasForeignKey(e => e.AchievementId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.EarnedAt).IsDescending();
        });
    }
}
