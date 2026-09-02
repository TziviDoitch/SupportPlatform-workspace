namespace SupportPlatform.Application.Metadata;

/// <summary>The named reference lists that fill the dynamic form's code-list controls.</summary>
public record ReferencesDto(
    IReadOnlyList<ReferenceItemDto> Domains,
    IReadOnlyList<ReferenceItemDto> BodyTypes,
    IReadOnlyList<ReferenceItemDto> Statuses,
    IReadOnlyList<ReferenceItemDto> Districts);
