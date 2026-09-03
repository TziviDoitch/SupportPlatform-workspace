using SupportPlatform.Application.Common;

namespace SupportPlatform.Application.Identity;

/// <summary>
/// Resolves the tenant a request may act on. Since S8 the caller's identity is authoritative
/// (<c>docs/contracts/api-contract.md</c> §Auth, <c>docs/ARCHITECTURE.md</c> §8.1): the only tenant
/// they can reach is <see cref="ICurrentUser.TenantId"/>. A request that names a different tenant
/// in its body or query string is a <see cref="ForbiddenException"/> (403); one that names none
/// inherits the caller's. The <c>X-User</c> header is still the identity source in the PoC — no JWT.
/// </summary>
public sealed class TenantAccessGuard(ICurrentUser user)
{
    /// <summary>
    /// Returns the tenant id the request may use, or throws <see cref="ForbiddenException"/> when
    /// <paramref name="requestedTenantId"/> is set and is not the caller's.
    /// </summary>
    public string EnsureTenant(string? requestedTenantId)
    {
        if (string.IsNullOrWhiteSpace(requestedTenantId))
            return user.TenantId;

        if (!string.Equals(requestedTenantId, user.TenantId, StringComparison.Ordinal))
            throw new ForbiddenException($"Tenant '{requestedTenantId}' is not accessible to the current user.");

        return requestedTenantId;
    }
}
