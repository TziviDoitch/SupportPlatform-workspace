using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.SavedQueries.Interfaces;

/// <summary>CRUD + re-run for saved queries, scoped to the current user + tenant.</summary>
public interface ISavedQueryService
{
    Task<IReadOnlyList<SavedQueryDto>> List(CancellationToken ct = default);
    Task<SavedQueryDto> Get(Guid id, CancellationToken ct = default);
    Task<SavedQueryDto> Create(SaveSavedQueryRequest request, CancellationToken ct = default);
    Task<SavedQueryDto> Update(Guid id, SaveSavedQueryRequest request, CancellationToken ct = default);
    Task Delete(Guid id, CancellationToken ct = default);

    /// <summary>Re-runs the stored definition and updates <c>lastRun*</c>; response mirrors <c>/api/search</c>.</summary>
    Task<SearchResponse> Run(Guid id, CancellationToken ct = default);
}
