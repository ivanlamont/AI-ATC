using AIATC.ScenarioService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AIATC.ScenarioService.Data;

/// <summary>
/// Database context for scenario usage data (sessions, scores, saved games)
/// </summary>
public class ScenarioUsageDbContext : DbContext
{
    public ScenarioUsageDbContext(DbContextOptions<ScenarioUsageDbContext> options)
        : base(options)
    {
    }

    public DbSet<Scenario> Scenarios { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<SavedScenario> SavedScenarios { get; set; }
    public DbSet<Score> Scores { get; set; }
    public DbSet<SessionCommand> SessionCommands { get; set; }
    public DbSet<SessionEvent> SessionEvents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scenario configuration
        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.HasIndex(e => e.AirportCode);
            entity.HasIndex(e => e.IsActive);
            entity.HasIndex(e => e.Difficulty);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email);
        });

        // Session configuration
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasIndex(e => e.ScenarioId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.StartedAt);
            entity.HasIndex(e => e.Status);

            entity.HasOne(s => s.Scenario)
                .WithMany(sc => sc.Sessions)
                .HasForeignKey(s => s.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.User)
                .WithMany(u => u.Sessions)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SavedScenario configuration
        modelBuilder.Entity<SavedScenario>(entity =>
        {
            entity.HasIndex(e => e.ScenarioId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SavedAt);

            entity.HasOne(ss => ss.Scenario)
                .WithMany(sc => sc.SavedScenarios)
                .HasForeignKey(ss => ss.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ss => ss.User)
                .WithMany(u => u.SavedScenarios)
                .HasForeignKey(ss => ss.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Score configuration
        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasIndex(e => e.ScenarioId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.ScenarioId, e.ScoreValue });
            entity.HasIndex(e => e.CompletedAt);

            entity.HasOne(sc => sc.Scenario)
                .WithMany(s => s.Scores)
                .HasForeignKey(sc => sc.ScenarioId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sc => sc.User)
                .WithMany(u => u.Scores)
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SessionCommand configuration
        modelBuilder.Entity<SessionCommand>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.CommandType);

            entity.HasOne(sc => sc.Session)
                .WithMany(s => s.Commands)
                .HasForeignKey(sc => sc.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SessionEvent configuration
        modelBuilder.Entity<SessionEvent>(entity =>
        {
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.EventType);
            entity.HasIndex(e => e.Severity);

            entity.HasOne(se => se.Session)
                .WithMany(s => s.Events)
                .HasForeignKey(se => se.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
