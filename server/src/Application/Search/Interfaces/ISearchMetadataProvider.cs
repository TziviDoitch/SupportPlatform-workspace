namespace SupportPlatform.Application.Search.Interfaces;

/// <summary>Supplies the <see cref="SearchMetadata"/> for the current request.</summary>
public interface ISearchMetadataProvider
{
    Task<SearchMetadata> Get(CancellationToken ct = default);
}
