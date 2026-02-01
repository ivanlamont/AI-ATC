using AIATC.Domain.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace AIATC.Domain.Data;

/// <summary>
/// Database context for AI-ATC application
/// </summary>
public class AircraftControlDbContext : DbContext
{
    public AircraftControlDbContext(DbContextOptions<AircraftControlDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Users table
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Authentication tokens table
    /// </summary>
    public DbSet<AuthToken> AuthTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AircraftControlDbContext).Assembly);
    }
}
