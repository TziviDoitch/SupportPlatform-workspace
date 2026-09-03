namespace SupportPlatform.Application.Search;

/// <summary>
/// Orders the aggregated buckets (by <see cref="QueryDefinition.Sort"/>, else by the segmentation
/// fields ascending) and cuts the requested page. Pure in-memory shaping of the query result —
/// kept in the Application layer, out of the EF executor.
/// </summary>
public static class BucketPaging
{
    private static readonly IComparer<object> KeyComparer = Comparer<object>.Default;

    public static QueryExecutionResult Apply(IReadOnlyList<AggregateBucket> buckets, QueryDefinition def)
    {
        var ordered = Order(buckets, def).ToList();
        var page = ordered
            .Skip((def.Paging.PageNumber - 1) * def.Paging.PageSize)
            .Take(def.Paging.PageSize)
            .ToList();

        return new QueryExecutionResult(page, ordered.Count);
    }

    private static IEnumerable<AggregateBucket> Order(IReadOnlyList<AggregateBucket> buckets, QueryDefinition def)
    {
        var keys = def.Sort.Count > 0
            ? def.Sort.Select(s => (Select: Selector(s.Field), Ascending: s.Direction == "asc"))
            : def.Segmentation.Select(id => (Select: KeyOf(id), Ascending: true));

        IOrderedEnumerable<AggregateBucket>? ordered = null;
        foreach (var (select, ascending) in keys)
            ordered = (ordered, ascending) switch
            {
                (null, true) => buckets.OrderBy(select, KeyComparer),
                (null, false) => buckets.OrderByDescending(select, KeyComparer),
                (not null, true) => ordered.ThenBy(select, KeyComparer),
                (not null, false) => ordered.ThenByDescending(select, KeyComparer)
            };

        return ordered ?? buckets.AsEnumerable();
    }

    private static Func<AggregateBucket, object> Selector(string field) => field switch
    {
        Metric.Count => b => b.Count,
        Metric.SumAmountApproved => b => b.SumAmountApproved,
        _ => KeyOf(field)
    };

    private static Func<AggregateBucket, object> KeyOf(string fieldId) => b => b.Key[fieldId];
}
