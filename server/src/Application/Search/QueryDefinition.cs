namespace SupportPlatform.Application.Search;

// The single canonical query object: the search form builds it, the NL parser emits it, a saved
// query stores it, DynamicQueryBuilder translates it, QuestionTextRenderer reads it.
public sealed record QueryDefinition
{
    public required string TenantId { get; init; }

    // Keys are canonical field ids from filter_field_registry; any unknown key is rejected (400).
    public required IReadOnlyDictionary<string, FilterValue> Filters { get; init; }

    public IReadOnlyList<string> Segmentation { get; init; } = [];

    public IReadOnlyList<string> Metrics { get; init; } = [];

    public Paging Paging { get; init; } = Paging.Default;

    public IReadOnlyList<SortSpec> Sort { get; init; } = [];

    public IReadOnlyList<string> EffectiveMetrics => Metrics.Count == 0 ? [Metric.Count] : Metrics;
}
