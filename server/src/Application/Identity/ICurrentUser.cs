namespace SupportPlatform.Application.Identity;

// The caller of the current request. PoC seam: identity comes from an X-User header; S8 replaces
// the source with a JWT.
public interface ICurrentUser
{
    string Username { get; }
    string TenantId { get; }
    string Role { get; }
    string CorrelationId { get; }
}
