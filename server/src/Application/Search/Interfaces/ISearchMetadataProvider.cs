namespace SupportPlatform.Application.Search.Interfaces;

public interface ISearchMetadataProvider
{
    Task<SearchMetadata> Get(CancellationToken ct = default);
}
