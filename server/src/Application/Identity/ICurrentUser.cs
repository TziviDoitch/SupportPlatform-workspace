namespace SupportPlatform.Application.Identity;

/// <summary>
/// The caller of the current request. PoC seam: identity comes from an <c>X-User</c> header
/// (<c>docs/contracts/api-contract.md</c> §Auth); S8 replaces the source with a JWT. Real
/// authorization beyond owner + tenant scoping is out of scope for S5.
/// </summary>
public interface ICurrentUser
{
    string Username { get; }
    string TenantId { get; }
    string Role { get; }

    /// <summary>Correlation id of the current request — stamped onto every audit record.</summary>
    string CorrelationId { get; }
}
