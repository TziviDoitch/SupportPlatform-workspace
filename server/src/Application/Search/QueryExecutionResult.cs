namespace SupportPlatform.Application.Search;

/// <summary>
/// The ordered aggregation of one search, in the two shapes the response needs:
/// <paramref name="Buckets"/> is the requested page (what <c>rows</c> shows), while
/// <paramref name="Ordered"/> is every group the query produced (what <c>aggregations</c> and
/// therefore the charts and header totals are computed from).
/// </summary>
public sealed record QueryExecutionResult(
    IReadOnlyList<AggregateBucket> Buckets,
    IReadOnlyList<AggregateBucket> Ordered)
{
    /// <summary>Number of groups before paging.</summary>
    public int TotalBuckets => Ordered.Count;
}
