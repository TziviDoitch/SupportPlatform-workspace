namespace SupportPlatform.Application.Search;

/// <summary>
/// One aggregated group produced by <c>ISearchQueryExecutor</c>: the group key (field id → value;
/// empty for no segmentation) plus every metric, computed regardless of which were requested.
/// </summary>
public sealed record AggregateBucket(
    IReadOnlyDictionary<string, object> Key,
    long Count,
    decimal SumAmountApproved)
{
    /// <summary>
    /// This bucket's value for one <see cref="Metric"/> name — the single place a metric name maps
    /// to a value, so a metric added to <see cref="Metric.All"/> and forgotten here fails loudly
    /// here instead of surfacing as a missing group key somewhere downstream.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="metric"/> is not a known metric.</exception>
    // The (object) cast is load-bearing: without it the switch arms unify on their best common
    // type (decimal), and `count` would box as a decimal instead of the long it is.
    public object Value(string metric) => metric switch
    {
        Metric.Count => (object)Count,
        Metric.SumAmountApproved => SumAmountApproved,
        _ => throw new ArgumentOutOfRangeException(
            nameof(metric), metric, $"'{metric}' is not a known metric.")
    };
}
