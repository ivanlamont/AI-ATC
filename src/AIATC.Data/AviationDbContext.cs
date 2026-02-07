using Microsoft.EntityFrameworkCore;
using AIATC.Domain.Models.Aviation;

namespace AIATC.Data;

/// <summary>
/// Entity Framework DbContext for Aviation data models
/// Supports both in-memory testing and SQL Server production use
/// </summary>
public class AviationDbContext : DbContext
{
    public AviationDbContext(DbContextOptions<AviationDbContext> options) : base(options)
    {
    }

    // Core Aviation Entities
    public DbSet<AircraftType> AircraftTypes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aircraft Type Configuration
        modelBuilder.Entity<AircraftType>(entity =>
        {
            entity.HasKey(at => at.IcaoCode);
            entity.Property(at => at.IcaoCode).HasMaxLength(6).IsRequired();
            entity.Property(at => at.Name).HasMaxLength(80);
        });

        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Default to in-memory for development/testing
            optionsBuilder.UseInMemoryDatabase("AviationDb");
        }
    }
}