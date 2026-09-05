namespace SupportPlatform.Application.Metadata.Interfaces;

public interface IMetadataRepository
{
    Task<MetadataSnapshot> GetSnapshot(CancellationToken ct = default);
}
