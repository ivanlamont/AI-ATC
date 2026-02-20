using Microsoft.EntityFrameworkCore;
using AIATC.Domain.Entities;
using DomainAircraftType = AIATC.Domain.Models.Aviation.AircraftType;

// NOTE: ARINC 424 model references have been removed.
// For airspace reference data, use:
//   - AIATC.ReferenceData.Models (scaffolded entities)
//   - AIATC.ReferenceData.Context.AirspaceReferenceDbContext (read-only context)

namespace AIATC.Data;

/// <summary>
/// Entity Framework DbContext for Game/Usage data (scenarios, sessions, scores)
///
/// IMPORTANT: This context no longer contains ARINC 424 aviation data.
/// For airspace reference data (airports, waypoints, procedures), use AirspaceReferenceDbContext.
///
/// Migration Path:
/// - ARINC 424 data → AirspaceReferenceDbContext (read-only, from external reference DB on port 5430)
/// - Scenario usage data → This context (read-write, PostgreSQL on port 4360)
///
/// Configured for PostgreSQL with environment-specific port handling (4360 for dev, 5432 for production)
/// </summary>
public class AviationDbContext : DbContext
{
    public AviationDbContext(DbContextOptions<AviationDbContext> options) : base(options)
    {
    }

    // ===== Game/Session Entities (from AIATC.Domain.Entities) =====
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

    // ===== Core Aviation Entities (Domain models used by game) =====
    public DbSet<DomainAircraftType> AircraftTypes => Set<DomainAircraftType>();

    // ===== ARINC 424 Models REMOVED =====
    // All ARINC 424 airspace reference data (airports, waypoints, procedures, airways, etc.)
    // has been moved to AirspaceReferenceDbContext for better separation of concerns.
    //
    // To access reference data:
    //   - Inject AirspaceReferenceDbContext (from AIATC.ReferenceData.Context)
    //   - Use scaffolded models from AIATC.ReferenceData.Models
    //
    // Example:
    //   var airport = await _airspaceDb.Airports
    //       .FirstOrDefaultAsync(a => a.IcaoCode == "KSFO");
    //
    // The reference database runs on port 5430 and contains proven ARINC 424 data
    // from the cycle2508 schema.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== Aircraft Type Configuration =====
        modelBuilder.Entity<DomainAircraftType>(entity =>
        {
            entity.HasKey(at => at.IcaoCode);
            entity.Property(at => at.IcaoCode).HasMaxLength(6).IsRequired();
            entity.Property(at => at.Name).HasMaxLength(80);
            entity.ToTable("aircraft_types");
        });

        // ===== UserAchievement Composite Key =====
        modelBuilder.Entity<UserAchievement>(entity =>
        {
            entity.HasKey(ua => new { ua.UserId, ua.AchievementId });
        });

        // ===== Game Entity Configurations =====
        // Most entity configurations are in separate IEntityTypeConfiguration<T> classes
        // in the AIATC.Domain project (e.g., Configurations/UserConfiguration.cs)
        // They will be auto-applied via ApplyConfigurationsFromAssembly

        // Apply all IEntityTypeConfiguration implementations from the Domain assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(User).Assembly);

        // Note: ARINC 424 model configurations have been removed.
        // Reference data is now managed by AirspaceReferenceDbContext.
    }

    // Helper methods for ARINC 424 complex type configurations have been removed.
    // These were specific to the old AIATC.Data models which are no longer used in this context.
    // For reference data configurations, see AirspaceReferenceDbContext in AIATC.ReferenceData.Context.

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Environment-specific PostgreSQL configuration
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var port = env == "Development" ? "4360" : "5432";
            var host = env == "Development" ? "localhost" : "postgres";
            var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "aiatc_dev_password";
            
            var connectionString = $"Host={host};Port={port};Database=aiatc;Username=aiatc;Password={password}";
            
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
            });

            // Enable sensitive data logging in development
            if (env == "Development")
            {
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
        }
    }
}