using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

/// <summary>
/// Base for a filter behaviour. One subclass per registry <c>kind</c> (<see cref="FieldKind"/>);
/// one instance per registry field, carrying that field's strongly-typed column selector. The
/// same instance filters (<see cref="Apply"/>), groups by that column in the database
/// (<see cref="AggregateGroups"/>), and provides the in-memory group key (<see cref="GroupKey"/>).
/// </summary>
public abstract class FilterHandler(string fieldId)
{
    private Func<SupportRequest, object>? _compiledGroupKey;

    /// <summary>Registry field id this instance serves, e.g. "bodyType".</summary>
    public string FieldId { get; } = fieldId;

    /// <summary>Registry kind this subclass implements, e.g. "codeList".</summary>
    public abstract string Kind { get; }

    /// <summary>Selector for the column this field groups by (string code or int year).</summary>
    public abstract LambdaExpression GroupKeySelector { get; }

    /// <summary>Boxed, compiled form of <see cref="GroupKeySelector"/> for in-memory grouping.</summary>
    public Func<SupportRequest, object> GroupKey =>
        _compiledGroupKey ??= Expression.Lambda<Func<SupportRequest, object>>(
            Expression.Convert(GroupKeySelector.Body, typeof(object)),
            GroupKeySelector.Parameters).Compile();

    /// <summary>Guard the value shape against <see cref="Kind"/>, then append the predicate.</summary>
    public IQueryable<SupportRequest> Apply(IQueryable<SupportRequest> source, FilterValue value)
    {
        Guard(value);
        return source.Where(BuildPredicate(value));
    }

    /// <summary>Group <paramref name="source"/> by this field's column in the database, one row per group.</summary>
    public abstract Task<IReadOnlyList<GroupAggregate>> AggregateGroups(
        IQueryable<SupportRequest> source, CancellationToken ct);

    protected abstract void Guard(FilterValue value);

    protected abstract Expression<Func<SupportRequest, bool>> BuildPredicate(FilterValue value);

    protected InvalidQueryException Invalid(string message) => new($"filters.{FieldId}", message);

    /// <summary>
    /// Shared <c>GroupBy</c> the subclasses call with their typed column. The sum is taken over
    /// <c>double</c> so the SQLite test provider can translate the aggregate; SQL Server keeps
    /// native <c>decimal</c>. Amounts are small enough for this to stay exact to the cent.
    /// </summary>
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
