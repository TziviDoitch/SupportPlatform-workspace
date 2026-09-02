namespace SupportPlatform.Application.Metadata.Interfaces;

/// <summary>Builds the <see cref="MetadataResponse"/> for a tenant.</summary>
public interface IMetadataService
{
    Task<MetadataResponse> Get(string tenantId, CancellationToken ct = default);
}
