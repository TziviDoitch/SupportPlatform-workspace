using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SupportPlatform.Infrastructure.Persistence.Interfaces;

namespace SupportPlatform.Infrastructure.Persistence;

// Used only by `dotnet ef` at design time. The connection string is a placeholder — EF never
// opens it to scaffold a migration.
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SupportPlatformDbContext>
{
    public SupportPlatformDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SupportPlatformDbContext>()
            .UseSqlServer("Server=localhost;Database=SupportPlatform;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new SupportPlatformDbContext(options, new DesignTimeTenantContext());
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public string? TenantId => null;
        public bool HasTenant => false;
        public void SetTenant(string tenantId) { }
    }
}
