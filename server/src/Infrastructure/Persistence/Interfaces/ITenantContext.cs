namespace SupportPlatform.Infrastructure.Persistence.Interfaces;

// Holds the tenant scope for the current request. The EF Core global query filter is fail-closed:
// when HasTenant is false, tenant-scoped entities return no rows.
public interface ITenantContext
{
    string? TenantId { get; }
    bool HasTenant { get; }
    void SetTenant(string tenantId);
}
