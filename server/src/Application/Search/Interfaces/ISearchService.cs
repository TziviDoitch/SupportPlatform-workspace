namespace SupportPlatform.Application.Search.Interfaces;

/// <summary>Validates and runs a <see cref="QueryDefinition"/>, returning the full search response.</summary>
public interface ISearchService
{
    Task<SearchResponse> Search(QueryDefinition definition, CancellationToken ct = default);
}
