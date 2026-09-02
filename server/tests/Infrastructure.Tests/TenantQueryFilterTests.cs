using Microsoft.EntityFrameworkCore;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Tests;

public class TenantQueryFilterTests
{
    [Fact]
    public void No_tenant_scope_returns_no_rows_fail_closed()
    {
        using var db = Seeded();
        db.Tenant.Clear();

        Assert.False(db.Context.SupportRequests.Any());
        Assert.False(db.Context.SubmittingBodies.Any());
    }

    [Fact]
    public void Scope_limits_rows_to_that_tenant()
    {
        using var db = Seeded();

        db.Tenant.SetTenant("culture-sport-admin");
        Assert.Equal(320, db.Context.SupportRequests.Count());
        Assert.All(db.Context.SupportRequests, r => Assert.Equal("culture-sport-admin", r.TenantId));

        db.Context.ChangeTracker.Clear();
        db.Tenant.SetTenant("welfare-admin");
        Assert.Equal(180, db.Context.SupportRequests.Count());
        Assert.All(db.Context.SupportRequests, r => Assert.Equal("welfare-admin", r.TenantId));
    }

    [Fact]
    public void IgnoreQueryFilters_sees_every_tenant()
    {
        using var db = Seeded();
        db.Tenant.Clear();

        Assert.Equal(500, db.Context.SupportRequests.IgnoreQueryFilters().Count());
    }

    private static TestDb Seeded()
    {
        var db = new TestDb();
        DbSeeder.Seed(db.Context);
        db.Context.ChangeTracker.Clear();
        return db;
    }
}
