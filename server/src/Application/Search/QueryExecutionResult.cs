namespace SupportPlatform.Application.Search;

// Buckets is the requested page (feeds `rows`); Ordered is every group the query produced
// (feeds `aggregations`, and so the charts and header totals).
public sealed record QueryExecutionResult(
    IReadOnlyList<AggregateBucket> Buckets,
    IReadOnlyList<AggregateBucket> Ordered)
{
    public int TotalBuckets => Ordered.Count;
}
