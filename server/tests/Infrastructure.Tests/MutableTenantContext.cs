using SupportPlatform.Infrastructure.Persistence.Interfaces;

namespace SupportPlatform.Infrastructure.Tests;

/// <summary>Test double for <see cref="ITenantContext"/> whose scope can be changed between queries.</summary>
public sealed class MutableTenantContext : ITenantContext
{
    public string? TenantId { get; private set; }
    public bool HasTenant => TenantId is not null;
    public void SetTenant(string tenantId) => TenantId = tenantId;
    public void Clear() => TenantId = null;
}
