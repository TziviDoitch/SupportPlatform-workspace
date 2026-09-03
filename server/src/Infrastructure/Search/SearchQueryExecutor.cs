using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Persistence.Interfaces;
using SupportPlatform.Infrastructure.Search.Filters;
using SupportPlatform.Infrastructure.Search.Filters.Interfaces;

namespace SupportPlatform.Infrastructure.Search;

/// <summary>
/// EF Core execution of a validated <see cref="QueryDefinition"/>: apply the tenant scope, the
/// whitelisted filters (<see cref="DynamicQueryBuilder"/>), then aggregate.
///
/// Aggregation (PoC — see docs/ARCHITECTURE.md §4):
/// <list type="bullet">
///   <item>0 segmentation fields → one aggregate computed in the database.</item>
///   <item>1 segmentation field → <c>GroupBy</c> in the database.</item>
///   <item>2+ fields → minimal materialization + in-memory grouping.</item>
/// </list>
/// Sums are taken over <c>double</c> so the SQLite test provider can translate the aggregate;
/// SQL Server would keep native <c>decimal</c>. Amounts are small enough that this is exact to the cent.
/// </summary>
public sealed class SearchQueryExecutor(
    SupportPlatformDbContext db,
    ITenantContext tenant,
    DynamicQueryBuilder builder,
    IFilterHandlerResolver handlers) : ISearchQueryExecutor
{
    private const string KeySeparator = "";
    private static readonly IComparer<object> KeyComparer = Comparer<object>.Default;

    public async Task<QueryExecutionResult> Execute(
        QueryDefinition definition,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        CancellationToken ct = default)
    {
        // The service has already validated that this tenant exists; apply the scope.
        tenant.SetTenant(definition.TenantId);

        var filtered = builder.Apply(db.SupportRequests.AsNoTracking(), definition, registry);

        var buckets = definition.Segmentation.Count switch
        {
            0 => await NoSegmentation(filtered, ct),
            1 => await OneSegment(filtered, registry, definition.Segmentation[0], ct),
            _ => await ManySegments(filtered, registry, definition.Segmentation, ct)
        };

        var ordered = Order(buckets, definition).ToList();
        var page = ordered
            .Skip((definition.Paging.PageNumber - 1) * definition.Paging.PageSize)
            .Take(definition.Paging.PageSize)
            .ToList();

        return new QueryExecutionResult(page, ordered.Count);
    }

    private static async Task<List<AggregateBucket>> NoSegmentation(IQueryable<SupportRequest> q, CancellationToken ct)
    {
        var count = await q.CountAsync(ct);
        // Nullable selector: SUM over zero matching rows is SQL NULL, not 0.
        var sum = await q.SumAsync(x => (double?)x.AmountApproved, ct) ?? 0d;
        return [new AggregateBucket(new Dictionary<string, object>(), count, (decimal)sum)];
    }

    private async Task<List<AggregateBucket>> OneSegment(
        IQueryable<SupportRequest> q,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        string fieldId,
        CancellationToken ct)
    {
        var handler = handlers.Resolve(FieldEntry(registry, fieldId));
        return handler switch
        {
            CodeListFilterHandler h => await GroupByColumn(q, h.Column, fieldId, ct),
            YearRangeFilterHandler h => await GroupByColumn(q, h.Column, fieldId, ct),
            _ => throw new InvalidOperationException($"Kind '{handler.Kind}' cannot be a segmentation key.")
        };
    }

    private static async Task<List<AggregateBucket>> GroupByColumn<TKey>(
        IQueryable<SupportRequest> q,
        Expression<Func<SupportRequest, TKey>> column,
        string fieldId,
        CancellationToken ct)
    {
        var raw = await q.GroupBy(column)
            .Select(g => new { g.Key, Count = g.Count(), Sum = g.Sum(x => (double)x.AmountApproved) })
            .ToListAsync(ct);

        return raw
            .Select(r => new AggregateBucket(
                new Dictionary<string, object> { [fieldId] = r.Key! }, r.Count, (decimal)r.Sum))
            .ToList();
    }

    private async Task<List<AggregateBucket>> ManySegments(
        IQueryable<SupportRequest> q,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        IReadOnlyList<string> fieldIds,
        CancellationToken ct)
    {
        var segHandlers = fieldIds.Select(id => handlers.Resolve(FieldEntry(registry, id))).ToList();

        var rows = await q.Include(x => x.SubmittingBody).ToListAsync(ct);

        return rows
            .GroupBy(r => CompositeKey(segHandlers, r))
            .Select(g =>
            {
                var first = g.First();
                var key = new Dictionary<string, object>();
                for (var i = 0; i < fieldIds.Count; i++)
                    key[fieldIds[i]] = segHandlers[i].GroupKey(first);
                return new AggregateBucket(key, g.LongCount(), g.Sum(r => r.AmountApproved));
            })
            .ToList();
    }

    private static string CompositeKey(IReadOnlyList<FilterHandler> segHandlers, SupportRequest row) =>
        string.Join(KeySeparator, segHandlers.Select(h => h.GroupKey(row)));

    private static FilterFieldRegistryEntry FieldEntry(IReadOnlyList<FilterFieldRegistryEntry> registry, string id) =>
        registry.FirstOrDefault(e => e.Id == id)
        ?? throw new InvalidQueryException($"segmentation.{id}", $"'{id}' is not a known filter field.");

    private static IEnumerable<AggregateBucket> Order(List<AggregateBucket> buckets, QueryDefinition def)
    {
        if (def.Sort.Count > 0)
            return ApplySort(buckets, def.Sort);

        IOrderedEnumerable<AggregateBucket>? ordered = null;
        foreach (var id in def.Segmentation)
            ordered = ordered is null
                ? buckets.OrderBy(b => b.Key[id], KeyComparer)
                : ordered.ThenBy(b => b.Key[id], KeyComparer);
        return ordered ?? buckets.AsEnumerable();
    }

    private static IEnumerable<AggregateBucket> ApplySort(List<AggregateBucket> buckets, IReadOnlyList<SortSpec> sort)
    {
        IOrderedEnumerable<AggregateBucket>? ordered = null;
        foreach (var spec in sort)
        {
            Func<AggregateBucket, object> selector = spec.Field switch
            {
                Metric.Count => b => b.Count,
                Metric.SumAmountApproved => b => b.SumAmountApproved,
                _ => b => b.Key[spec.Field]
            };
            var asc = spec.Direction == "asc";
            ordered = (ordered, asc) switch
            {
                (null, true) => buckets.OrderBy(selector, KeyComparer),
                (null, false) => buckets.OrderByDescending(selector, KeyComparer),
                (not null, true) => ordered.ThenBy(selector, KeyComparer),
                (not null, false) => ordered.ThenByDescending(selector, KeyComparer)
            };
        }
        return ordered ?? buckets.AsEnumerable();
    }
}
