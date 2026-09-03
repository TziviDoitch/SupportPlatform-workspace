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
/// EF Core execution of a validated <see cref="QueryDefinition"/>: apply the tenant scope and the
/// whitelisted filters (<see cref="DynamicQueryBuilder"/>), then aggregate every group. Ordering
/// and paging are done afterwards by <see cref="BucketPaging"/> in the Application layer.
///
/// Aggregation (PoC — see docs/ARCHITECTURE.md §4):
/// <list type="bullet">
///   <item>0 segmentation fields → one aggregate computed in the database.</item>
///   <item>1 segmentation field → <c>GroupBy</c> in the database (via the field's handler).</item>
///   <item>2+ fields → minimal materialization + in-memory grouping.</item>
/// </list>
/// </summary>
public sealed class SearchQueryExecutor(
    SupportPlatformDbContext db,
    ITenantContext tenant,
    DynamicQueryBuilder builder,
    IFilterHandlerResolver handlers) : ISearchQueryExecutor
{
    private const string KeySeparator = "|"; // no reference code or year contains a pipe

    public async Task<IReadOnlyList<AggregateBucket>> Execute(
        QueryDefinition definition,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        CancellationToken ct = default)
    {
        // The service has already validated that this tenant exists; apply the scope.
        tenant.SetTenant(definition.TenantId);

        var filtered = builder.Apply(db.SupportRequests.AsNoTracking(), definition, registry);

        return definition.Segmentation.Count switch
        {
            0 => await NoSegmentation(filtered, ct),
            1 => await OneSegment(filtered, HandlersFor(registry, definition.Segmentation), ct),
            _ => await ManySegments(filtered, HandlersFor(registry, definition.Segmentation), ct)
        };
    }

    private static async Task<List<AggregateBucket>> NoSegmentation(IQueryable<SupportRequest> q, CancellationToken ct)
    {
        var count = await q.CountAsync(ct);
        // Nullable selector: SUM over zero matching rows is SQL NULL, not 0.
        var sum = await q.SumAsync(x => (double?)x.AmountApproved, ct) ?? 0d;
        return [new AggregateBucket(new Dictionary<string, object>(), count, (decimal)sum)];
    }

    private static async Task<List<AggregateBucket>> OneSegment(
        IQueryable<SupportRequest> q, IReadOnlyList<FilterHandler> segHandlers, CancellationToken ct)
    {
        var handler = segHandlers[0];
        var groups = await handler.AggregateGroups(q, ct);

        return groups
            .Select(g => new AggregateBucket(
                new Dictionary<string, object> { [handler.FieldId] = g.Key }, g.Count, g.SumAmountApproved))
            .ToList();
    }

    private static async Task<List<AggregateBucket>> ManySegments(
        IQueryable<SupportRequest> q, IReadOnlyList<FilterHandler> segHandlers, CancellationToken ct)
    {
        var rows = await q.Include(x => x.SubmittingBody).ToListAsync(ct);

        return rows
            .GroupBy(r => CompositeKey(segHandlers, r))
            .Select(g =>
            {
                var first = g.First();
                var key = segHandlers.ToDictionary(h => h.FieldId, h => h.GroupKey(first));
                return new AggregateBucket(key, g.LongCount(), g.Sum(r => r.AmountApproved));
            })
            .ToList();
    }

    private IReadOnlyList<FilterHandler> HandlersFor(
        IReadOnlyList<FilterFieldRegistryEntry> registry, IReadOnlyList<string> fieldIds) =>
        fieldIds.Select(id => handlers.Resolve(FieldEntry(registry, id))).ToList();

    private static string CompositeKey(IReadOnlyList<FilterHandler> segHandlers, SupportRequest row) =>
        string.Join(KeySeparator, segHandlers.Select(h => h.GroupKey(row)));

    private static FilterFieldRegistryEntry FieldEntry(IReadOnlyList<FilterFieldRegistryEntry> registry, string id) =>
        registry.FirstOrDefault(e => e.Id == id)
        ?? throw new InvalidQueryException($"segmentation.{id}", $"'{id}' is not a known filter field.");
}
