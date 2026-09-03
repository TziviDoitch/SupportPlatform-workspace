namespace SupportPlatform.Application.Search;

/// <summary>
/// Result of <c>POST /api/search</c>. Shape frozen in <c>docs/contracts/api-contract.md</c> §3.
/// </summary>
public sealed record SearchResponse(
    string QuestionText,
    IReadOnlyList<IReadOnlyDictionary<string, object>> Rows,
    IReadOnlyList<AggregationDto> Aggregations,
    PageDto Page,
    ExecutionMetaDto ExecutionMeta);
