using System.Linq.Expressions;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

/// <summary>
/// Base for a filter behaviour. One subclass per registry <c>kind</c> (<see cref="FieldKind"/>);
/// one instance per registry field, carrying that field's strongly-typed column selector. The
/// same instance both filters (<see cref="Apply"/>) and provides the segmentation key
/// (<see cref="GroupKeySelector"/> / <see cref="GroupKey"/>).
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

    protected abstract void Guard(FilterValue value);

    protected abstract Expression<Func<SupportRequest, bool>> BuildPredicate(FilterValue value);

    protected InvalidQueryException Invalid(string message) => new($"filters.{FieldId}", message);
}
