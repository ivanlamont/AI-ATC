using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AIATC.BFF;

/// <summary>
/// Minimal EF Core context used exclusively for persisting ASP.NET Core DataProtection
/// keys to the shared PostgreSQL database. Keeping this separate from ScenarioService's
/// DbContexts avoids coupling the BFF to the scenario data model.
/// </summary>
public class BffDbContext : DbContext, IDataProtectionKeyContext
{
    public BffDbContext(DbContextOptions<BffDbContext> options) : base(options) { }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
}
