using FluentValidation;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Application.Search.Validation;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Tests.Search;

public class SearchServiceTests
{
    private readonly FakeExecutor _executor = new();
    private readonly SearchService _service;

    public SearchServiceTests()
    {
        _service = new SearchService(
            TestMetadata.Provider,
            new QueryDefinitionValidator(TestMetadata.Provider),
            _executor,
            new QuestionTextRenderer());
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
        var def = Valid() with { TenantId = "nope" };

        await Assert.ThrowsAsync<ValidationException>(() => _service.Search(def));

        Assert.False(_executor.WasCalled);
    }

    [Fact]
    public async Task Maps_buckets_paging_and_execution_meta()
    {
        _executor.Result = new QueryExecutionResult(
        [
            new AggregateBucket(new Dictionary<string, object> { ["supportYear"] = 2023 }, 12, 1000m),
            new AggregateBucket(new Dictionary<string, object> { ["supportYear"] = 2024 }, 8, 500m)
        ], TotalBuckets: 2);

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
        _executor.Result = new QueryExecutionResult(
            [new AggregateBucket(new Dictionary<string, object>(), 12, 1000m)], TotalBuckets: 1);

        var countOnly = await _service.Search(Valid() with { Segmentation = [], Metrics = [] });
        Assert.Equal(new[] { "count" }, countOnly.Aggregations[0].Metrics.Keys);

        var both = await _service.Search(Valid() with { Segmentation = [], Metrics = ["count", "sumAmountApproved"] });
        Assert.Equal(new[] { "count", "sumAmountApproved" }, both.Aggregations[0].Metrics.Keys);
        Assert.Equal(1000m, both.Aggregations[0].Metrics["sumAmountApproved"]);
    }

    private sealed class FakeExecutor : ISearchQueryExecutor
    {
        public bool WasCalled { get; private set; }

        public QueryExecutionResult Result { get; set; } =
            new([new AggregateBucket(new Dictionary<string, object>(), 0, 0m)], 1);

        public Task<QueryExecutionResult> Execute(
            QueryDefinition definition, IReadOnlyList<FilterFieldRegistryEntry> registry, CancellationToken ct = default)
        {
            WasCalled = true;
            return Task.FromResult(Result);
        }
    }
}
