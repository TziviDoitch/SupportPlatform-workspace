using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Search;
using SupportPlatform.Infrastructure.Search.Filters;

namespace SupportPlatform.Infrastructure.Tests.Search;

public class SearchQueryExecutorTests
{
    private static (TestDb Db, SearchQueryExecutor Executor) Arrange()
    {
        var db = new TestDb();
        DbSeeder.Seed(db.Context);
        db.Context.ChangeTracker.Clear();

        var resolver = new FilterHandlerResolver(FilterHandlers.Default);
        var executor = new SearchQueryExecutor(
            db.Context, db.Tenant, new DynamicQueryBuilder(resolver), resolver);
        return (db, executor);
    }

    private static QueryDefinition Def(string tenant, IReadOnlyList<string> segmentation) => new()
    {
        TenantId = tenant,
        Filters = new Dictionary<string, FilterValue>(),
        Segmentation = segmentation
    };

    [Fact]
    public async Task No_segmentation_returns_one_bucket_with_the_tenant_totals()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var result = await executor.Execute(Def("culture-sport-admin", []), registry);

        var bucket = Assert.Single(result.Buckets);
        Assert.Empty(bucket.Key);
        Assert.Equal(320, bucket.Count);
        Assert.True(bucket.SumAmountApproved > 0);
        Assert.Equal(1, result.TotalBuckets);
    }

    [Fact]
    public async Task A_filter_that_matches_no_rows_returns_a_zero_bucket()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var def = Def("culture-sport-admin", []) with
        {
            Filters = new Dictionary<string, FilterValue>
            {
                ["supportYear"] = new FilterValue.YearSingle(1990)
            }
        };

        var result = await executor.Execute(def, registry);

        var bucket = Assert.Single(result.Buckets);
        Assert.Equal(0, bucket.Count);
        Assert.Equal(0m, bucket.SumAmountApproved);
    }

    [Fact]
    public async Task Tenant_scope_comes_from_the_definition()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var result = await executor.Execute(Def("welfare-admin", []), registry);

        Assert.Equal(180, Assert.Single(result.Buckets).Count);
    }

    [Fact]
    public async Task One_segmentation_field_groups_in_the_database()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var result = await executor.Execute(Def("culture-sport-admin", ["supportYear"]), registry);

        Assert.Equal(3, result.Buckets.Count);
        Assert.Equal(new object[] { 2023, 2024, 2025 }, result.Buckets.Select(b => b.Key["supportYear"]));
        Assert.Equal(320, result.Buckets.Sum(b => b.Count));
    }

    [Fact]
    public async Task Two_segmentation_fields_group_in_memory()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var result = await executor.Execute(Def("culture-sport-admin", ["supportYear", "district"]), registry);

        Assert.Equal(320, result.Buckets.Sum(b => b.Count));
        Assert.All(result.Buckets, b =>
        {
            Assert.Contains("supportYear", b.Key.Keys);
            Assert.Contains("district", b.Key.Keys);
        });
        Assert.Equal(result.Buckets.Count, result.TotalBuckets);
    }

    [Fact]
    public async Task Paging_limits_the_returned_buckets_but_reports_the_full_total()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var def = Def("culture-sport-admin", ["supportYear"]) with { Paging = new Paging(2, 1) };
        var result = await executor.Execute(def, registry);

        Assert.Equal(2, result.Buckets.Count);
        Assert.Equal(3, result.TotalBuckets);
    }

    [Fact]
    public async Task Sort_descending_reverses_the_default_key_order()
    {
        var (db, executor) = Arrange();
        using var _ = db;
        var registry = db.Context.FilterFieldRegistry.AsNoTracking().ToList();

        var def = Def("culture-sport-admin", ["supportYear"]) with { Sort = [new SortSpec("supportYear", "desc")] };
        var result = await executor.Execute(def, registry);

        Assert.Equal(new object[] { 2025, 2024, 2023 }, result.Buckets.Select(b => b.Key["supportYear"]));
    }
}
