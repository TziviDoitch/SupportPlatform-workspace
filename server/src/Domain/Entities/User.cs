namespace SupportPlatform.Domain.Entities;

// Seed user for the JWT auth planned in S8. S1 only stores the data; no auth logic yet.
public class User
{
    public Guid Id { get; set; }
    public required string Username { get; set; }

    // Deterministic salted hash — never a plaintext password.
    public required string PasswordHash { get; set; }

    public required string TenantId { get; set; }
    public required string Role { get; set; }
}
