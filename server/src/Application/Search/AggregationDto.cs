namespace SupportPlatform.Application.Search;

/// <summary>
/// One segmentation group: <see cref="Key"/> echoes the grouped field ids (empty when there is
/// no segmentation), <see cref="Metrics"/> holds the requested metric values.
/// </summary>
public sealed record AggregationDto(
    IReadOnlyDictionary<string, object> Key,
    IReadOnlyDictionary<string, object> Metrics);
