using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Tests;

public class SeedPasswordHasherTests
{
    [Fact]
    public void Same_input_produces_the_same_hash()
    {
        Assert.Equal(
            SeedPasswordHasher.Hash("sarah", "pass"),
            SeedPasswordHasher.Hash("sarah", "pass"));
    }

    [Fact]
    public void Hash_is_not_the_plaintext()
    {
        var hash = SeedPasswordHasher.Hash("sarah", "pass");
        Assert.DoesNotContain("pass", hash[(hash.LastIndexOf('$') + 1)..]);
        Assert.StartsWith("pbkdf2-sha256$100000$", hash);
    }

    [Fact]
    public void Different_username_or_password_changes_the_hash()
    {
        var baseline = SeedPasswordHasher.Hash("sarah", "pass");
        Assert.NotEqual(baseline, SeedPasswordHasher.Hash("dan", "pass"));
        Assert.NotEqual(baseline, SeedPasswordHasher.Hash("sarah", "other"));
    }
}
