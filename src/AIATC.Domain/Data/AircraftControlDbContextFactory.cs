using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AIATC.Domain.Data;

/// <summary>
/// Design-time factory for AircraftControlDbContext
/// Used by EF Core tools for migrations
/// </summary>
public class AircraftControlDbContextFactory : IDesignTimeDbContextFactory<AircraftControlDbContext>
{
    public AircraftControlDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AircraftControlDbContext>();

        // Use a default connection string for migrations
        // In production, this should come from configuration
        var connectionString = "Host=localhost;Database=aiatc;Username=aiatc;Password=aiatc";

        optionsBuilder.UseNpgsql(connectionString);

        return new AircraftControlDbContext(optionsBuilder.Options);
    }
}
