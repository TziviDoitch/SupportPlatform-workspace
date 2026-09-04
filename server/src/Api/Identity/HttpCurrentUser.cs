using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Identity;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Api.Identity;

/// <summary>
/// Resolves <see cref="ICurrentUser"/> from the <c>X-User</c> request header against the seeded
/// users. A missing or unrecognized header falls back to the default seed user — the PoC has no
/// real authentication (that is S8). Resolved once per request and cached.
///
/// The identity and its tenant are always read from a real <c>users</c> row: with no row to fall
/// back to, this throws rather than inventing a username and tenant.
/// </summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor, SupportPlatformDbContext db) : ICurrentUser
{
    public const string HeaderName = "X-User";
    private const string DefaultUsername = "sarah";

    private (string Username, string TenantId, string Role)? _resolved;

    public string Username => Resolve().Username;
    public string TenantId => Resolve().TenantId;
    public string Role => Resolve().Role;

    public string CorrelationId => accessor.HttpContext?.TraceIdentifier ?? string.Empty;

    private (string Username, string TenantId, string Role) Resolve()
    {
        if (_resolved is { } cached)
            return cached;

        var requested = accessor.HttpContext?.Request.Headers[HeaderName].ToString();
        var name = string.IsNullOrWhiteSpace(requested) ? DefaultUsername : requested;

        var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Username == name)
                   ?? db.Users.AsNoTracking().FirstOrDefault(u => u.Username == DefaultUsername)
                   ?? throw new InvalidOperationException(
                       $"No user row resolves the request: '{HeaderName}: {name}' is unknown and the " +
                       $"default seed user '{DefaultUsername}' is missing. The identity and its tenant " +
                       "always come from the database — never from a hard-coded fallback.");

        _resolved = (user.Username, user.TenantId, user.Role);
        return _resolved.Value;
    }
}
