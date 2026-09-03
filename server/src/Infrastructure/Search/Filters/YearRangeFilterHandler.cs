using System.Linq.Expressions;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

/// <summary>
/// Handles <c>kind: "yearRange"</c> — an inclusive range or an exact year over an int column.
/// The range/single split is a closed two-case match on the value shape, not an extension point.
/// </summary>
public sealed class YearRangeFilterHandler(
    string fieldId,
    Expression<Func<SupportRequest, int>> column) : FilterHandler(fieldId)
{
    /// <summary>The year column, e.g. <c>r =&gt; r.SupportYear</c>. Also the group key.</summary>
    public Expression<Func<SupportRequest, int>> Column { get; } = column;

    public override string Kind => FieldKind.YearRange;

    public override LambdaExpression GroupKeySelector => Column;

    public override Task<IReadOnlyList<GroupAggregate>> AggregateGroups(
        IQueryable<SupportRequest> source, CancellationToken ct) => AggregateBy(source, Column, ct);

    protected override void Guard(FilterValue value)
    {
        switch (value)
        {
            case FilterValue.YearRange r when r.From > r.To:
                throw Invalid("'from' must be less than or equal to 'to'.");
            case FilterValue.YearRange or FilterValue.YearSingle:
                return;
            default:
                throw Invalid("Expected a year range or a single year.");
        }
    }

    protected override Expression<Func<SupportRequest, bool>> BuildPredicate(FilterValue value) => value switch
    {
        FilterValue.YearRange r => FilterPredicates.YearBetween(Column, r.From, r.To),
        FilterValue.YearSingle s => FilterPredicates.YearEquals(Column, s.Value),
        _ => throw Invalid("Expected a year range or a single year.")
    };
}
