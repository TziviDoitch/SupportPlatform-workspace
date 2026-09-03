using System.Linq.Expressions;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

/// <summary>Handles <c>kind: "codeList"</c> — IN over a string code column.</summary>
public sealed class CodeListFilterHandler(
    string fieldId,
    Expression<Func<SupportRequest, string>> column) : FilterHandler(fieldId)
{
    /// <summary>The code column, e.g. <c>r =&gt; r.SubmittingBody!.BodyTypeCode</c>. Also the group key.</summary>
    public Expression<Func<SupportRequest, string>> Column { get; } = column;

    public override string Kind => FieldKind.CodeList;

    public override LambdaExpression GroupKeySelector => Column;

    protected override void Guard(FilterValue value)
    {
        if (value is not FilterValue.Codes { Values.Count: > 0 } codes || codes.Values.Any(string.IsNullOrWhiteSpace))
            throw Invalid("Expected one or more non-empty codes.");
    }

    protected override Expression<Func<SupportRequest, bool>> BuildPredicate(FilterValue value) =>
        FilterPredicates.CodeIn(Column, ((FilterValue.Codes)value).Values);
}
