using System.Diagnostics;
using FluentValidation;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.Search;

/// <summary>
/// The S2 use case: validate a <see cref="QueryDefinition"/>, run it, and shape the response
/// (question text, rows, aggregations, paging, execution meta). All business decisions for
/// <c>POST /api/search</c> live here; the controller only forwards.
/// </summary>
public sealed class SearchService(
    ISearchMetadataProvider metadata,
    IValidator<QueryDefinition> validator,
    ISearchQueryExecutor executor,
    QuestionTextRenderer questionText) : ISearchService
{
    public async Task<SearchResponse> Search(QueryDefinition definition, CancellationToken ct = default)
    {
        var result = await validator.ValidateAsync(definition, ct);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var meta = await metadata.Get(ct);

        var watch = Stopwatch.StartNew();
        var buckets = await executor.Execute(definition, meta.Registry, ct);
        var execution = BucketPaging.Apply(buckets, definition);
        watch.Stop();

        var metrics = definition.EffectiveMetrics;

        return new SearchResponse(
            QuestionText: questionText.Render(definition, meta.Snapshot),
            Rows: execution.Buckets.Select(b => Row(b, metrics)).ToList(),
            Aggregations: execution.Buckets
                .Select(b => new AggregationDto(b.Key, Metrics(b, metrics)))
                .ToList(),
            Page: new PageDto(definition.Paging.PageNumber, definition.Paging.PageSize, execution.TotalBuckets),
            ExecutionMeta: new ExecutionMetaDto(
                DurationMs: watch.ElapsedMilliseconds,
                RowCount: execution.Buckets.Count,
                CacheHit: false,
                DefinitionHash: DefinitionHasher.Hash(definition)));
    }

    private static IReadOnlyDictionary<string, object> Metrics(AggregateBucket bucket, IReadOnlyList<string> requested)
    {
        var values = new Dictionary<string, object>();
        foreach (var m in requested)
            values[m] = m switch
            {
                Metric.Count => bucket.Count,
                Metric.SumAmountApproved => bucket.SumAmountApproved,
                _ => throw new ArgumentOutOfRangeException(nameof(requested), m, "unknown metric")
            };
        return values;
    }

    private static IReadOnlyDictionary<string, object> Row(AggregateBucket bucket, IReadOnlyList<string> requested)
    {
        var row = new Dictionary<string, object>(bucket.Key);
        foreach (var (name, value) in Metrics(bucket, requested))
            row[name] = value;
        return row;
    }
}
