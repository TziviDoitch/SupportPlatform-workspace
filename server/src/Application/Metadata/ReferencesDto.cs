namespace SupportPlatform.Application.Metadata;

public record ReferencesDto(
    IReadOnlyList<ReferenceItemDto> Domains,
    IReadOnlyList<ReferenceItemDto> BodyTypes,
    IReadOnlyList<ReferenceItemDto> Statuses,
    IReadOnlyList<ReferenceItemDto> Districts);
