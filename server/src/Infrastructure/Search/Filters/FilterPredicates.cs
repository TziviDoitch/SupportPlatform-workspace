using System.Linq.Expressions;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

// Builds Expression predicates over SupportRequest from a field's typed column selector. No
// string parsing, no name reflection — the selectors are lambdas supplied at registration.
internal static class FilterPredicates
{
    // MethodInfo via a delegate, not a magic string — Enumerable.Contains<string>.
    private static readonly System.Reflection.MethodInfo Contains =
        new Func<IEnumerable<string>, string, bool>(Enumerable.Contains).Method;

    public static Expression<Func<SupportRequest, bool>> CodeIn(
        Expression<Func<SupportRequest, string>> column, IReadOnlyList<string> codes)
    {
        // r => codes.Contains(r.<column>)  ->  SQL: WHERE <column> IN (...)
        var body = Expression.Call(
            Contains,
            Expression.Constant(codes.ToArray(), typeof(IEnumerable<string>)),
            column.Body);
        return Expression.Lambda<Func<SupportRequest, bool>>(body, column.Parameters[0]);
    }

    public static Expression<Func<SupportRequest, bool>> YearBetween(
        Expression<Func<SupportRequest, int>> column, int from, int to)
    {
        var body = Expression.AndAlso(
            Expression.GreaterThanOrEqual(column.Body, Expression.Constant(from)),
            Expression.LessThanOrEqual(column.Body, Expression.Constant(to)));
        return Expression.Lambda<Func<SupportRequest, bool>>(body, column.Parameters[0]);
    }

    public static Expression<Func<SupportRequest, bool>> YearEquals(
        Expression<Func<SupportRequest, int>> column, int value)
    {
        var body = Expression.Equal(column.Body, Expression.Constant(value));
        return Expression.Lambda<Func<SupportRequest, bool>>(body, column.Parameters[0]);
    }
}
