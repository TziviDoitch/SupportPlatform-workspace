namespace SupportPlatform.Application.Search;

/// <summary>
/// One aggregated group produced by <c>ISearchQueryExecutor</c>: the group key (field id → value;
/// empty for no segmentation) plus every metric, computed regardless of which were requested.
/// </summary>
public sealed record AggregateBucket(
    IReadOnlyDictionary<string, object> Key,
    long Count,
    decimal SumAmountApproved);
