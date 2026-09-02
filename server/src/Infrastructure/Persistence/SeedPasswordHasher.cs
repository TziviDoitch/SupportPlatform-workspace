using System.Security.Cryptography;
using System.Text;

namespace SupportPlatform.Infrastructure.Persistence;

/// <summary>
/// Deterministic password hashing for seed data only. A plaintext password is never stored.
/// The salt is derived from the username so the same seed run always produces the same hash,
/// keeping <see cref="DbSeeder"/> reproducible. S8 owns the production hash/verify scheme and
/// may replace this entirely.
/// </summary>
public static class SeedPasswordHasher
{
    private const int Iterations = 100_000;
    private const int KeyBytes = 32;

    public static string Hash(string username, string password)
    {
        var salt = SHA256.HashData(Encoding.UTF8.GetBytes(username));
        var key = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password), salt, Iterations, HashAlgorithmName.SHA256, KeyBytes);
        return $"pbkdf2-sha256${Iterations}${Convert.ToBase64String(key)}";
    }
}
