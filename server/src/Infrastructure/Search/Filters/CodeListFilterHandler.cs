using System.Linq.Expressions;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

// Handles kind: "codeList" — IN over a string code column.
public sealed class CodeListFilterHandler(
    string fieldId,
    Expression<Func<SupportRequest, string>> column) : FilterHandler(fieldId)
{
    public Expression<Func<SupportRequest, string>> Column { get; } = column;

    public override string Kind => FieldKind.CodeList;

    public override LambdaExpression GroupKeySelector => Column;

    public override Task<IReadOnlyList<GroupAggregate>> AggregateGroups(
        IQueryable<SupportRequest> source, CancellationToken ct) => AggregateBy(source, Column, ct);

    protected override void Guard(FilterValue value)
    {
        if (value is not FilterValue.Codes { Values.Count: > 0 } codes || codes.Values.Any(string.IsNullOrWhiteSpace))
            throw Invalid("Expected one or more non-empty codes.");
    }

    protected override Expression<Func<SupportRequest, bool>> BuildPredicate(FilterValue value) =>
        FilterPredicates.CodeIn(Column, ((FilterValue.Codes)value).Values);
}
