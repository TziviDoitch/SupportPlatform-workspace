using Microsoft.EntityFrameworkCore;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence.Interfaces;

namespace SupportPlatform.Infrastructure.Persistence;

/// <summary>
/// EF Core context for the whole model. Tenant-scoped entities carry a <b>fail-closed</b>
/// global query filter (see <see cref="ITenantContext"/>): with no tenant scope set, they
/// return no rows. Use <c>IgnoreQueryFilters()</c> explicitly for tests or admin paths.
/// </summary>
public class SupportPlatformDbContext(
    DbContextOptions<SupportPlatformDbContext> options,
    ITenantContext tenant) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<SubmittingBody> SubmittingBodies => Set<SubmittingBody>();
    public DbSet<SupportRequest> SupportRequests => Set<SupportRequest>();
    public DbSet<ReferenceDomain> ReferenceDomains => Set<ReferenceDomain>();
    public DbSet<ReferenceBodyType> ReferenceBodyTypes => Set<ReferenceBodyType>();
    public DbSet<ReferenceStatus> ReferenceStatuses => Set<ReferenceStatus>();
    public DbSet<ReferenceDistrict> ReferenceDistricts => Set<ReferenceDistrict>();
    public DbSet<FilterFieldRegistryEntry> FilterFieldRegistry => Set<FilterFieldRegistryEntry>();
    public DbSet<SavedQuery> SavedQueries => Set<SavedQuery>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SupportPlatformDbContext).Assembly);

        // Fail-closed tenant isolation: no scope -> no rows.
        modelBuilder.Entity<SubmittingBody>()
            .HasQueryFilter(e => tenant.HasTenant && e.TenantId == tenant.TenantId);
        modelBuilder.Entity<SupportRequest>()
            .HasQueryFilter(e => tenant.HasTenant && e.TenantId == tenant.TenantId);
    }
}
