namespace SupportPlatform.Infrastructure.Persistence.Interfaces;

/// <summary>
/// Holds the tenant scope for the current request. The EF Core global query filter is
/// <b>fail-closed</b>: when <see cref="HasTenant"/> is <c>false</c>, tenant-scoped entities
/// return no rows. In S1 the scope is set from the dev-only <c>?tenantId=</c> parameter;
/// in S8 it is set from the authenticated user's tenant.
/// </summary>
public interface ITenantContext
{
    /// <summary>The active tenant id, or <c>null</c> when no scope has been set.</summary>
    string? TenantId { get; }

    /// <summary>True once a tenant scope has been set for this request.</summary>
    bool HasTenant { get; }

    void SetTenant(string tenantId);
}
