namespace SupportPlatform.Application.Search;

public sealed record AggregationDto(
    IReadOnlyDictionary<string, object> Key,
    IReadOnlyDictionary<string, object> Metrics);
