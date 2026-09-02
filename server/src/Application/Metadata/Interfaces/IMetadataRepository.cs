namespace SupportPlatform.Application.Metadata.Interfaces;

/// <summary>Reads the reference lists and the filter-field registry. Both are global for the PoC.</summary>
public interface IMetadataRepository
{
    Task<MetadataSnapshot> GetSnapshot(CancellationToken ct = default);
}
