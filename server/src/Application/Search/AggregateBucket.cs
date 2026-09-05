namespace SupportPlatform.Application.Search;

public sealed record AggregateBucket(
    IReadOnlyDictionary<string, object> Key,
    long Count,
    decimal SumAmountApproved)
{
    // The (object) cast is load-bearing: without it the switch arms unify on decimal and
    // `Count` boxes as a decimal instead of the long it is.
    public object Value(string metric) => metric switch
    {
        Metric.Count => (object)Count,
        Metric.SumAmountApproved => SumAmountApproved,
        _ => throw new ArgumentOutOfRangeException(
            nameof(metric), metric, $"'{metric}' is not a known metric.")
    };
}
