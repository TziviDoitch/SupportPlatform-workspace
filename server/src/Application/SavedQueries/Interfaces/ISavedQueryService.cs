using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.SavedQueries.Interfaces;

public interface ISavedQueryService
{
    Task<IReadOnlyList<SavedQueryDto>> List(CancellationToken ct = default);
    Task<SavedQueryDto> Get(Guid id, CancellationToken ct = default);
    Task<SavedQueryDto> Create(SaveSavedQueryRequest request, CancellationToken ct = default);
    Task<SavedQueryDto> Update(Guid id, SaveSavedQueryRequest request, CancellationToken ct = default);
    Task Delete(Guid id, CancellationToken ct = default);
    Task<SearchResponse> Run(Guid id, CancellationToken ct = default);
}
