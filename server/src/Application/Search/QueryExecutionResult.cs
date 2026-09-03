namespace SupportPlatform.Application.Search;

/// <summary>The page of aggregated buckets plus the total bucket count before paging.</summary>
public sealed record QueryExecutionResult(IReadOnlyList<AggregateBucket> Buckets, int TotalBuckets);
