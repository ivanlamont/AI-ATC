using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using AIATC.Data;

namespace AIATC.EF
{
    public class AviationDbContextFactory : IDesignTimeDbContextFactory<AviationDbContext>
    {
        public AviationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AviationDbContext>();
            // Use a default connection string for migrations
            var connectionString = "Host=localhost;Port=4360;Database=aiatc;Username=aiatc;Password=aiatc_dev_password";
            optionsBuilder.UseNpgsql(connectionString, options =>
            {
                options.MigrationsAssembly("AIATC.EF");
                options.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null);
            });
            return new AviationDbContext(optionsBuilder.Options);
        }
    }
}
