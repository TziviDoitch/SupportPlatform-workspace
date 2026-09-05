namespace SupportPlatform.Application.Search.Interfaces;

public interface ISearchService
{
    Task<SearchResponse> Search(QueryDefinition definition, CancellationToken ct = default);
}
