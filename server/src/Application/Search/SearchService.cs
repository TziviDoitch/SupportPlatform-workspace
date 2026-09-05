using System.Diagnostics;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.Search;

// Validates a QueryDefinition, runs it, and shapes the response. Identical definitions (by
// canonical hash) are served from an in-memory cache with executionMeta.cacheHit = true; every
// run is audited.
public sealed class SearchService(
    ISearchMetadataProvider metadata,
    IValidator<QueryDefinition> validator,
    ISearchQueryExecutor executor,
    QuestionTextRenderer questionText,
    IMemoryCache cache,
    SearchCacheOptions cacheOptions,
    TenantAccessGuard tenantAccess,
    IAuditService audit) : ISearchService
{
    public async Task<SearchResponse> Search(QueryDefinition definition, CancellationToken ct = default)
    {
        // Identity is authoritative for the tenant: fill it in when omitted, 403 on a mismatch.
        definition = definition with { TenantId = tenantAccess.EnsureTenant(definition.TenantId) };

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
            // rows = the requested page; aggregations = every group.
            Rows: execution.Buckets.Select(b => Row(b, metrics)).ToList(),
            Aggregations: execution.Ordered
                .Select(b => new AggregationDto(b.Key, Metrics(b, metrics)))
                .ToList(),
            Page: new PageDto(definition.Paging.PageNumber, definition.Paging.PageSize, execution.TotalBuckets),
            ExecutionMeta: new ExecutionMetaDto(
                DurationMs: watch.ElapsedMilliseconds,
                RowCount: execution.Buckets.Count,
                CacheHit: false,
                DefinitionHash: hash));

        if (caching)
            cache.Set(hash, response, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheOptions.Ttl,
                Size = 1
            });
        return response;
    }

    private static IReadOnlyDictionary<string, object> Metrics(AggregateBucket bucket, IReadOnlyList<string> requested)
    {
        var values = new Dictionary<string, object>();
        foreach (var m in requested)
            values[m] = bucket.Value(m);
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
