namespace SupportPlatform.Application.Search;

public sealed record SearchResponse(
    string QuestionText,
    IReadOnlyList<IReadOnlyDictionary<string, object>> Rows,
    IReadOnlyList<AggregationDto> Aggregations,
    PageDto Page,
    ExecutionMetaDto ExecutionMeta);
