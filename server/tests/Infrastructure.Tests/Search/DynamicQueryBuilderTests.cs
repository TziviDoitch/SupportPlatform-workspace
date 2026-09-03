using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Search;
using SupportPlatform.Infrastructure.Search.Filters;
using SupportPlatform.Infrastructure.Search.Filters.Interfaces;

namespace SupportPlatform.Infrastructure.Tests.Search;

public class DynamicQueryBuilderTests
{
    private const string Tenant = "culture-sport-admin";

    private static (TestDb Db, DynamicQueryBuilder Builder, IReadOnlyList<FilterFieldRegistryEntry> Registry) Arrange()
    {
        var db = new TestDb();
        DbSeeder.Seed(db.Context);
        db.Context.ChangeTracker.Clear();
        db.Tenant.SetTenant(Tenant);

        var builder = new DynamicQueryBuilder(new FilterHandlerResolver(FilterHandlers.Default));
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();
        return (db, builder, registry);
    }

    private static QueryDefinition Def(params (string Key, FilterValue Value)[] filters) => new()
    {
        TenantId = Tenant,
        Filters = filters.ToDictionary(f => f.Key, f => f.Value)
    };

    [Fact]
    public void No_filters_returns_every_row_in_the_tenant_scope()
    {
        var arranged = Arrange();
        using var db = arranged.Db;
        var q = arranged.Builder.Apply(db.Context.SupportRequests, Def(), arranged.Registry);

        Assert.Equal(320, q.Count());
    }

    [Fact]
    public void Code_list_filter_on_the_request_narrows_to_matching_codes()
    {
        var arranged = Arrange();
        using var db = arranged.Db;
        var q = arranged.Builder.Apply(
            db.Context.SupportRequests, Def(("status", new FilterValue.Codes(["approved"]))), arranged.Registry);

        Assert.All(q.ToList(), r => Assert.Equal("approved", r.StatusCode));
        Assert.Equal(db.Context.SupportRequests.Count(r => r.StatusCode == "approved"), q.Count());
    }

    [Fact]
    public void Code_list_filter_across_the_submitting_body_navigation_works()
    {
        var arranged = Arrange();
        using var db = arranged.Db;
        var expected = db.Context.SupportRequests.Count(r => r.SubmittingBody!.DistrictCode == "north");

        var q = arranged.Builder.Apply(
            db.Context.SupportRequests, Def(("district", new FilterValue.Codes(["north"]))), arranged.Registry);

        Assert.Equal(expected, q.Count());
        Assert.True(expected > 0);
    }

    [Fact]
    public void Year_range_is_inclusive_and_excludes_years_outside_it()
    {
        var arranged = Arrange();
        using var db = arranged.Db;
        var q = arranged.Builder.Apply(
            db.Context.SupportRequests, Def(("supportYear", new FilterValue.YearRange(2023, 2024))), arranged.Registry);

        Assert.All(q.ToList(), r => Assert.InRange(r.SupportYear, 2023, 2024));
        Assert.Equal(db.Context.SupportRequests.Count(r => r.SupportYear <= 2024), q.Count());
    }

    [Fact]
    public void Single_year_matches_exactly_that_year()
    {
        var arranged = Arrange();
        using var db = arranged.Db;
        var q = arranged.Builder.Apply(
            db.Context.SupportRequests, Def(("supportYear", new FilterValue.YearSingle(2025))), arranged.Registry);

        Assert.All(q.ToList(), r => Assert.Equal(2025, r.SupportYear));
    }

    [Fact]
    public void Multiple_filters_are_combined_with_and()
    {
        var arranged = Arrange();
        using var db = arranged.Db;
        var q = arranged.Builder.Apply(
            db.Context.SupportRequests,
            Def(("status", new FilterValue.Codes(["approved"])), ("supportYear", new FilterValue.YearSingle(2025))),
            arranged.Registry);

        var expected = db.Context.SupportRequests.Count(r => r.StatusCode == "approved" && r.SupportYear == 2025);
        Assert.Equal(expected, q.Count());
        Assert.All(q.ToList(), r =>
        {
            Assert.Equal("approved", r.StatusCode);
            Assert.Equal(2025, r.SupportYear);
        });
    }

    [Fact]
    public void An_unknown_field_id_is_rejected_before_any_handler_runs()
    {
        var spy = new RecordingResolver();
        var builder = new DynamicQueryBuilder(spy);
        var arranged = Arrange();
        using var db = arranged.Db;

        var ex = Assert.Throws<InvalidQueryException>(() => builder.Apply(
            db.Context.SupportRequests,
            Def(("costCenter", new FilterValue.Codes(["x"]))),
            arranged.Registry));

        Assert.Equal("filters.costCenter", ex.Field);
        Assert.Equal(0, spy.ResolveCalls);
    }

    private sealed class RecordingResolver : IFilterHandlerResolver
    {
        public int ResolveCalls { get; private set; }

        public FilterHandler Resolve(FilterFieldRegistryEntry field)
        {
            ResolveCalls++;
            return FilterHandlers.Default[0];
        }
    }
}
