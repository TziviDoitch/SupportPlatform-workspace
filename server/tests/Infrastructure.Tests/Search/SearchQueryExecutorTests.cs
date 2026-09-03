using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Search;
using SupportPlatform.Infrastructure.Search.Filters;
using SupportPlatform.Infrastructure.Search.Filters.Interfaces;

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

    private static IReadOnlyList<FilterFieldRegistryEntry> Registry(TestDb db) =>
        db.Context.FilterFieldRegistry.AsNoTracking().ToList();

    [Fact]
    public async Task No_segmentation_returns_one_bucket_with_the_tenant_totals()
    {
        var (db, executor) = Arrange();
        using var _ = db;

        var buckets = await executor.Execute(Def("culture-sport-admin", []), Registry(db));

        var bucket = Assert.Single(buckets);
        Assert.Empty(bucket.Key);
        Assert.Equal(320, bucket.Count);
        Assert.True(bucket.SumAmountApproved > 0);
    }

    [Fact]
    public async Task A_filter_that_matches_no_rows_returns_a_zero_bucket()
    {
        var (db, executor) = Arrange();
        using var _ = db;

        var def = Def("culture-sport-admin", []) with
        {
            Filters = new Dictionary<string, FilterValue> { ["supportYear"] = new FilterValue.YearSingle(1990) }
        };

        var bucket = Assert.Single(await executor.Execute(def, Registry(db)));
        Assert.Equal(0, bucket.Count);
        Assert.Equal(0m, bucket.SumAmountApproved);
    }

    [Fact]
    public async Task Tenant_scope_comes_from_the_definition()
    {
        var (db, executor) = Arrange();
        using var _ = db;

        var buckets = await executor.Execute(Def("welfare-admin", []), Registry(db));

        Assert.Equal(180, Assert.Single(buckets).Count);
    }

    [Fact]
    public async Task One_segmentation_field_groups_by_that_column_in_the_database()
    {
        var (db, executor) = Arrange();
        using var _ = db;

        var buckets = await executor.Execute(Def("culture-sport-admin", ["supportYear"]), Registry(db));

        Assert.Equal(3, buckets.Count);
        Assert.Equal(
            new object[] { 2023, 2024, 2025 },
            buckets.Select(b => b.Key["supportYear"]).OrderBy(y => y));
        Assert.Equal(320, buckets.Sum(b => b.Count));
    }

    [Fact]
    public async Task Two_segmentation_fields_group_in_memory()
    {
        var (db, executor) = Arrange();
        using var _ = db;

        var buckets = await executor.Execute(
            Def("culture-sport-admin", ["supportYear", "district"]), Registry(db));

        Assert.Equal(320, buckets.Sum(b => b.Count));
        Assert.All(buckets, b =>
        {
            Assert.Contains("supportYear", b.Key.Keys);
            Assert.Contains("district", b.Key.Keys);
        });
    }
}
