using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Identity;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Api.Identity;

// Resolves ICurrentUser from the X-User header against the seeded users; a missing or unknown
// header falls back to the default seed user (the PoC has no real auth — that is S8). The
// identity and its tenant always come from a real users row; with none, this throws.
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
