using Microsoft.EntityFrameworkCore;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Tests;

public class DbSeederTests
{
    [Fact]
    public void Seeds_expected_row_counts()
    {
        using var db = new TestDb();
        DbSeeder.Seed(db.Context);

        Assert.Equal(2, db.Context.Tenants.Count());
        Assert.Equal(3, db.Context.Users.Count());
        Assert.Equal(5, db.Context.FilterFieldRegistry.Count());
        Assert.Equal(5, db.Context.ReferenceDomains.Count());
        Assert.Equal(4, db.Context.ReferenceBodyTypes.Count());
        Assert.Equal(4, db.Context.ReferenceStatuses.Count());
        Assert.Equal(3, db.Context.ReferenceDistricts.Count());
        Assert.Equal(40, db.Context.SubmittingBodies.IgnoreQueryFilters().Count());
        Assert.Equal(500, db.Context.SupportRequests.IgnoreQueryFilters().Count());
    }

    [Fact]
    public void Reference_lists_carry_the_values_the_assignment_enumerates()
    {
        using var db = new TestDb();
        DbSeeder.Seed(db.Context);

        AssertSameValues(
            ["עמותה", "רשות מקומית", "אגודה", "מוסד תרבות"],
            db.Context.ReferenceBodyTypes.ToList().Select(r => r.Label));

        AssertSameValues(
            ["תרבות", "ספורט", "מוזיאונים", "ספריות", "אירועי תרבות"],
            db.Context.ReferenceDomains.ToList().Select(r => r.Label));

        AssertSameValues(
            ["הוגשה", "בבדיקה", "אושרה", "נדחתה"],
            db.Context.ReferenceStatuses.ToList().Select(r => r.Label));
    }

    // The assignment lists values, not an order.
    private static void AssertSameValues(IEnumerable<string> expected, IEnumerable<string> actual) =>
        Assert.Equal(
            expected.OrderBy(v => v, StringComparer.Ordinal),
            actual.OrderBy(v => v, StringComparer.Ordinal));

    [Fact]
    public void Registry_matches_the_frozen_contract()
    {
        using var db = new TestDb();
        DbSeeder.Seed(db.Context);

        var registry = db.Context.FilterFieldRegistry.OrderBy(e => e.SortOrder).ToList();

        Assert.Equal(
            ["bodyType", "supportDomain", "status", "district", "supportYear"],
            registry.Select(e => e.Id));

        var year = registry.Single(e => e.Id == "supportYear");
        Assert.Equal("yearRange", year.Kind);
        Assert.Null(year.ReferenceList);
        Assert.Equal(["range", "single"], year.Operators);

        var status = registry.Single(e => e.Id == "status");
        Assert.Equal(["in"], status.Operators);
        Assert.False(status.Segmentable);
    }

    [Fact]
    public void Is_deterministic_across_runs()
    {
        var first = Fingerprint();
        var second = Fingerprint();
        Assert.Equal(first, second);

        static List<string> Fingerprint()
        {
            using var db = new TestDb();
            DbSeeder.Seed(db.Context);
            return db.Context.SupportRequests.IgnoreQueryFilters()
                .OrderBy(r => r.Id)
                .Select(r => $"{r.Id}|{r.TenantId}|{r.SubmittingBodyId}|{r.SupportDomainCode}|{r.StatusCode}|{r.SupportYear}|{r.AmountRequested}|{r.AmountApproved}")
                .ToList();
        }
    }

    [Fact]
    public void Is_idempotent()
    {
        using var db = new TestDb();
        DbSeeder.Seed(db.Context);
        DbSeeder.Seed(db.Context);

        Assert.Equal(500, db.Context.SupportRequests.IgnoreQueryFilters().Count());
        Assert.Equal(40, db.Context.SubmittingBodies.IgnoreQueryFilters().Count());
    }

    [Fact]
    public void Distribution_is_not_degenerate()
    {
        using var db = new TestDb();
        DbSeeder.Seed(db.Context);
        var rows = db.Context.SupportRequests.IgnoreQueryFilters().ToList();

        Assert.Equal(2, rows.Select(r => r.TenantId).Distinct().Count());
        Assert.Equal(3, rows.Select(r => r.SupportYear).Distinct().Count());
        Assert.Equal(4, rows.Select(r => r.StatusCode).Distinct().Count());
        Assert.Equal(5, rows.Select(r => r.SupportDomainCode).Distinct().Count());
        Assert.Equal(3, db.Context.SubmittingBodies.IgnoreQueryFilters()
            .Select(b => b.DistrictCode).Distinct().Count());
        Assert.Equal(4, db.Context.SubmittingBodies.IgnoreQueryFilters()
            .Select(b => b.BodyTypeCode).Distinct().Count());

        // Weighting holds: approved is the plurality status.
        var byStatus = rows.GroupBy(r => r.StatusCode).ToDictionary(g => g.Key, g => g.Count());
        var approved = byStatus["approved"];
        Assert.All(byStatus.Where(s => s.Key != "approved"),
            s => Assert.True(approved > s.Value, $"approved={approved} {s.Key}={s.Value}"));

        // Approved requests carry a positive approved amount; others carry zero.
        Assert.All(rows.Where(r => r.StatusCode == "approved"), r => Assert.True(r.AmountApproved > 0));
        Assert.All(rows.Where(r => r.StatusCode != "approved"), r => Assert.Equal(0m, r.AmountApproved));
    }

    [Fact]
    public void Users_carry_hashed_passwords_not_plaintext()
    {
        using var db = new TestDb();
        DbSeeder.Seed(db.Context);

        Assert.All(db.Context.Users, u =>
        {
            Assert.StartsWith("pbkdf2-sha256$", u.PasswordHash);
            Assert.NotEqual("pass", u.PasswordHash);
        });
    }
}
