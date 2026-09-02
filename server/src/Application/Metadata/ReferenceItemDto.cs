namespace SupportPlatform.Application.Metadata;

/// <summary>A reference (lookup) option: stable <paramref name="Code"/> + Hebrew <paramref name="Label"/>.</summary>
public record ReferenceItemDto(string Code, string Label);
