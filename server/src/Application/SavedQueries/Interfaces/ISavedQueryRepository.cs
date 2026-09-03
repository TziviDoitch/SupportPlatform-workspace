using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.SavedQueries.Interfaces;

/// <summary>
/// Persistence for <see cref="SavedQuery"/>. Every read is scoped to owner + tenant; a record
/// outside that scope is simply not found.
/// </summary>
public interface ISavedQueryRepository
{
    Task<IReadOnlyList<SavedQuery>> List(string ownerUsername, string tenantId, CancellationToken ct = default);
    Task<SavedQuery?> Find(Guid id, string ownerUsername, string tenantId, CancellationToken ct = default);
    Task Add(SavedQuery query, CancellationToken ct = default);
    Task Save(CancellationToken ct = default);
    Task Remove(SavedQuery query, CancellationToken ct = default);
}
