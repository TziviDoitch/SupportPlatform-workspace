using SupportPlatform.Application.Common;
using SupportPlatform.Application.Identity;

namespace SupportPlatform.Application.Tests.Identity;

public class TenantAccessGuardTests
{
    private static TenantAccessGuard Guard(string tenantId = "culture-sport-admin") =>
        new(new FakeCurrentUser("sarah", tenantId));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void No_tenant_requested_inherits_the_callers(string? requested)
    {
        Assert.Equal("culture-sport-admin", Guard().EnsureTenant(requested));
    }

    [Fact]
    public void The_callers_own_tenant_is_allowed()
    {
        Assert.Equal("culture-sport-admin", Guard().EnsureTenant("culture-sport-admin"));
    }

    [Fact]
    public void A_different_tenant_is_forbidden()
    {
        var error = Assert.Throws<ForbiddenException>(() => Guard().EnsureTenant("welfare-admin"));

        Assert.Contains("welfare-admin", error.Message);
    }
}
