namespace SupportPlatform.Application.Metadata.Interfaces;

public interface IMetadataService
{
    Task<MetadataResponse> Get(string tenantId, CancellationToken ct = default);
}
