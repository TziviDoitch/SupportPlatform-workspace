using SupportPlatform.Application.Common;

namespace SupportPlatform.Application.Identity;

// Resolves the tenant a request may act on. The caller's identity is authoritative: a request
// that names a different tenant is a ForbiddenException (403); one that names none inherits the
// caller's.
public sealed class TenantAccessGuard(ICurrentUser user)
{
    public string EnsureTenant(string? requestedTenantId)
    {
        if (string.IsNullOrWhiteSpace(requestedTenantId))
            return user.TenantId;

        if (!string.Equals(requestedTenantId, user.TenantId, StringComparison.Ordinal))
            throw new ForbiddenException($"Tenant '{requestedTenantId}' is not accessible to the current user.");

        return requestedTenantId;
    }
}
