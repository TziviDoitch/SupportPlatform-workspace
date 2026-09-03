using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using SupportPlatform.Application.Common;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Application.Search.Validation;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Tests.Search;

public class SearchServiceTests
{
    private readonly FakeExecutor _executor = new();
    private readonly RecordingAuditService _audit = new();
    private readonly SearchService _service;

    public SearchServiceTests()
    {
        _service = new SearchService(
            TestMetadata.Provider,
            new QueryDefinitionValidator(TestMetadata.Provider),
            _executor,
            new QuestionTextRenderer(),
            new MemoryCache(new MemoryCacheOptions()),
            new SearchCacheOptions(),
            new TenantAccessGuard(new FakeCurrentUser()),
            _audit);
    }

    private static QueryDefinition Valid() => new()
    {
        TenantId = "culture-sport-admin",
        Filters = new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["approved"]) },
        Segmentation = ["supportYear"]
    };

    [Fact]
    public async Task Invalid_definition_throws_and_never_touches_the_executor()
    {
        // Tenant is now authoritative from identity (guarded before validation), so the invalid
        // thing here is an unknown metric — still a ValidationException, executor untouched.
        var def = Valid() with { Metrics = ["bogus"] };

        await Assert.ThrowsAsync<ValidationException>(() => _service.Search(def));

        Assert.False(_executor.WasCalled);
    }

    [Fact]
    public async Task A_tenant_that_is_not_the_callers_is_forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.Search(Valid() with { TenantId = "welfare-admin" }));

        Assert.False(_executor.WasCalled);
    }

    [Fact]
    public async Task Maps_buckets_paging_and_execution_meta()
    {
        _executor.Result =
        [
            new AggregateBucket(new Dictionary<string, object> { ["supportYear"] = 2023 }, 12, 1000m),
            new AggregateBucket(new Dictionary<string, object> { ["supportYear"] = 2024 }, 8, 500m)
        ];

        var response = await _service.Search(Valid());

        Assert.Equal(2, response.Rows.Count);
        Assert.Equal(2, response.Aggregations.Count);
        Assert.Equal(2, response.Page.TotalRows);
        Assert.Equal(2, response.ExecutionMeta.RowCount);
        Assert.False(response.ExecutionMeta.CacheHit);
        Assert.StartsWith("sha256:", response.ExecutionMeta.DefinitionHash);
        Assert.NotEmpty(response.QuestionText);
    }

    [Fact]
    public async Task Aggregations_carry_only_the_requested_metrics()
    {
        _executor.Result = [new AggregateBucket(new Dictionary<string, object>(), 12, 1000m)];

        var countOnly = await _service.Search(Valid() with { Segmentation = [], Metrics = [] });
        Assert.Equal(new[] { "count" }, countOnly.Aggregations[0].Metrics.Keys);

        var both = await _service.Search(Valid() with { Segmentation = [], Metrics = ["count", "sumAmountApproved"] });
        Assert.Equal(new[] { "count", "sumAmountApproved" }, both.Aggregations[0].Metrics.Keys);
        Assert.Equal(1000m, both.Aggregations[0].Metrics["sumAmountApproved"]);
    }

    [Fact]
    public async Task Identical_definition_is_served_from_cache_on_the_second_run()
    {
        _executor.Result = [new AggregateBucket(new Dictionary<string, object>(), 5, 0m)];
        var first = await _service.Search(Valid() with { Segmentation = [] });
        Assert.False(first.ExecutionMeta.CacheHit);

        // A changed executor result would show through if the query actually re-ran.
        _executor.Result = [new AggregateBucket(new Dictionary<string, object>(), 999, 0m)];
        var second = await _service.Search(Valid() with { Segmentation = [] });

        Assert.True(second.ExecutionMeta.CacheHit);
        Assert.Equal(
            first.Aggregations[0].Metrics["count"],
            second.Aggregations[0].Metrics["count"]);
    }

    [Fact]
    public async Task A_different_definition_is_not_a_cache_hit()
    {
        _executor.Result = [new AggregateBucket(new Dictionary<string, object>(), 5, 0m)];
        await _service.Search(Valid() with { Segmentation = [] });

        var other = await _service.Search(
            Valid() with { Segmentation = [], Filters = new Dictionary<string, FilterValue>
            {
                ["status"] = new FilterValue.Codes(["pending"])
            } });

        Assert.False(other.ExecutionMeta.CacheHit);
    }

    [Fact]
    public async Task Every_search_is_audited()
    {
        _executor.Result = [new AggregateBucket(new Dictionary<string, object>(), 1, 0m)];

        await _service.Search(Valid() with { Segmentation = [] });

        Assert.Contains(_audit.Records, r => r.Action == "search" && r.EntityType == "QueryDefinition");
    }

    private sealed class FakeExecutor : ISearchQueryExecutor
    {
        public bool WasCalled { get; private set; }

        public IReadOnlyList<AggregateBucket> Result { get; set; } =
            [new AggregateBucket(new Dictionary<string, object>(), 0, 0m)];

        public Task<IReadOnlyList<AggregateBucket>> Execute(
            QueryDefinition definition, IReadOnlyList<FilterFieldRegistryEntry> registry, CancellationToken ct = default)
        {
            WasCalled = true;
            return Task.FromResult(Result);
        }
    }
}
