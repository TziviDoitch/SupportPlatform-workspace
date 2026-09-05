namespace SupportPlatform.Application.Metadata;

// Everything the client needs to build the search form, and the whitelist the server validates
// a QueryDefinition against.
public record MetadataResponse(
    string TenantId,
    ReferencesDto References,
    IReadOnlyList<FilterFieldDto> FilterFieldRegistry);
