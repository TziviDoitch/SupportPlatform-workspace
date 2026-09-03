using System.Diagnostics;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.Search;

/// <summary>
/// The S2 use case: validate a <see cref="QueryDefinition"/>, run it, and shape the response
/// (question text, rows, aggregations, paging, execution meta). All business decisions for
/// <c>POST /api/search</c> live here; the controller only forwards.
///
/// S5 adds dedup: identical definitions (by canonical <see cref="DefinitionHasher"/> hash) are
/// served from an in-memory cache with <c>executionMeta.cacheHit = true</c>, and every run is
/// recorded via <see cref="IAuditService"/>.
/// </summary>
public sealed class SearchService(
    ISearchMetadataProvider metadata,
    IValidator<QueryDefinition> validator,
    ISearchQueryExecutor executor,
    QuestionTextRenderer questionText,
    IMemoryCache cache,
    SearchCacheOptions cacheOptions,
    IAuditService audit) : ISearchService
{
    public async Task<SearchResponse> Search(QueryDefinition definition, CancellationToken ct = default)
    {
        var result = await validator.ValidateAsync(definition, ct);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        var hash = DefinitionHasher.Hash(definition);
        var caching = cacheOptions.TtlSeconds > 0;

        var response = caching && cache.TryGetValue(hash, out SearchResponse? cached) && cached is not null
            ? cached with { ExecutionMeta = cached.ExecutionMeta with { CacheHit = true } }
            : await Run(definition, hash, caching, ct);

        await audit.Record("search", "QueryDefinition", null, definition, ct);
        return response;
    }

    private async Task<SearchResponse> Run(QueryDefinition definition, string hash, bool caching, CancellationToken ct)
    {
        var meta = await metadata.Get(ct);

        var watch = Stopwatch.StartNew();
        var buckets = await executor.Execute(definition, meta.Registry, ct);
        var execution = BucketPaging.Apply(buckets, definition);
        watch.Stop();

        var metrics = definition.EffectiveMetrics;

        var response = new SearchResponse(
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
                DefinitionHash: hash));

        if (caching)
            cache.Set(hash, response, cacheOptions.Ttl);
        return response;
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
