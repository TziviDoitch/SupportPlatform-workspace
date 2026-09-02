using SupportPlatform.Infrastructure.Persistence.Interfaces;

namespace SupportPlatform.Infrastructure.Persistence;

/// <summary>Scoped, request-lifetime implementation of <see cref="ITenantContext"/>.</summary>
public class TenantContext : ITenantContext
{
    public string? TenantId { get; private set; }

    public bool HasTenant => TenantId is not null;

    public void SetTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ArgumentException("Tenant id must not be empty.", nameof(tenantId));

        TenantId = tenantId;
    }
}
