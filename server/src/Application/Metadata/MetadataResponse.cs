namespace SupportPlatform.Application.Metadata;

/// <summary>
/// Response of <c>GET /api/metadata</c>: everything the client needs to build the search form
/// and the whitelist the server validates a <c>QueryDefinition</c> against.
/// Shape frozen in <c>docs/contracts/metadata-model.md</c>.
/// </summary>
public record MetadataResponse(
    string TenantId,
    ReferencesDto References,
    IReadOnlyList<FilterFieldDto> FilterFieldRegistry);
