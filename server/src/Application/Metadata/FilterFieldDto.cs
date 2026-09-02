namespace SupportPlatform.Application.Metadata;

/// <summary>One entry of the filter-field registry (whitelist) as returned by <c>GET /api/metadata</c>.</summary>
public record FilterFieldDto(
    string Id,
    string Label,
    string Kind,
    string? ReferenceList,
    IReadOnlyList<string> Operators,
    bool Segmentable);
