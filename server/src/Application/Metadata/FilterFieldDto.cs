namespace SupportPlatform.Application.Metadata;

public record FilterFieldDto(
    string Id,
    string Label,
    string Kind,
    string? ReferenceList,
    IReadOnlyList<string> Operators,
    bool Segmentable);
