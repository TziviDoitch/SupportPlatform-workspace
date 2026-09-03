namespace SupportPlatform.Application.Search;

/// <summary>
/// The single canonical query object (§3.4, <c>docs/contracts/query-definition.md</c>): the search
/// form builds it, the NL parser will emit it, a saved query stores it, <c>DynamicQueryBuilder</c>
/// translates it, <see cref="QuestionTextRenderer"/> reads it.
/// </summary>
public sealed record QueryDefinition
{
    /// <summary>Organization scope. Must be a known tenant (validated).</summary>
    public required string TenantId { get; init; }

    /// <summary>
    /// Keys are canonical field ids from <c>filter_field_registry</c>. May be empty. Any key not
    /// in the registry is rejected (400) before the query is built.
    /// </summary>
    public required IReadOnlyDictionary<string, FilterValue> Filters { get; init; }

    /// <summary>Group-by field ids, in grouping order. Each must be a registry field with <c>segmentable: true</c>.</summary>
    public IReadOnlyList<string> Segmentation { get; init; } = [];

    /// <summary><c>count</c> and/or <c>sumAmountApproved</c>. Empty is treated as <c>["count"]</c>.</summary>
    public IReadOnlyList<string> Metrics { get; init; } = [];

    public Paging Paging { get; init; } = Paging.Default;

    public IReadOnlyList<SortSpec> Sort { get; init; } = [];

    /// <summary>Requested metrics with the <c>count</c> default applied.</summary>
    public IReadOnlyList<string> EffectiveMetrics => Metrics.Count == 0 ? [Metric.Count] : Metrics;
}
