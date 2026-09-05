using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

// One subclass per registry kind, one instance per registry field carrying that field's typed
// column selector. The same instance filters (Apply), groups by that column in the database
// (AggregateGroups), and provides the in-memory group key (GroupKey).
public abstract class FilterHandler(string fieldId)
{
    private Func<SupportRequest, object>? _compiledGroupKey;

    public string FieldId { get; } = fieldId;

    public abstract string Kind { get; }

    public abstract LambdaExpression GroupKeySelector { get; }

    // Boxed, compiled form of GroupKeySelector for in-memory grouping.
    public Func<SupportRequest, object> GroupKey =>
        _compiledGroupKey ??= Expression.Lambda<Func<SupportRequest, object>>(
            Expression.Convert(GroupKeySelector.Body, typeof(object)),
            GroupKeySelector.Parameters).Compile();

    public IQueryable<SupportRequest> Apply(IQueryable<SupportRequest> source, FilterValue value)
    {
        Guard(value);
        return source.Where(BuildPredicate(value));
    }

    public abstract Task<IReadOnlyList<GroupAggregate>> AggregateGroups(
        IQueryable<SupportRequest> source, CancellationToken ct);

    protected abstract void Guard(FilterValue value);

    protected abstract Expression<Func<SupportRequest, bool>> BuildPredicate(FilterValue value);

    protected InvalidQueryException Invalid(string message) => new($"filters.{FieldId}", message);

    // The sum is taken over double so the SQLite test provider can translate the aggregate;
    // SQL Server keeps native decimal. Amounts are small enough to stay exact to the cent.
    protected static async Task<IReadOnlyList<GroupAggregate>> AggregateBy<TKey>(
        IQueryable<SupportRequest> source,
        Expression<Func<SupportRequest, TKey>> column,
        CancellationToken ct)
    {
        var raw = await source.GroupBy(column)
            .Select(g => new { g.Key, Count = g.Count(), Sum = g.Sum(x => (double)x.AmountApproved) })
            .ToListAsync(ct);

        return raw.Select(r => new GroupAggregate(r.Key!, r.Count, (decimal)r.Sum)).ToList();
    }
}
