namespace SupportPlatform.Domain.Entities;

/// <summary>
/// A seed user. Prepared for the JWT authentication implemented in S8
/// (login → validate credentials against <see cref="PasswordHash"/> → issue JWT →
/// Bearer auth → identify user → resolve <see cref="TenantId"/> → authorization).
/// S1 only stores the data; it contains no authentication logic.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public required string Username { get; set; }

    /// <summary>Deterministic salted hash. A plaintext password is never stored.</summary>
    public required string PasswordHash { get; set; }

    public required string TenantId { get; set; }
    public required string Role { get; set; }
}
