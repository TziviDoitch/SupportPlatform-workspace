using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.SavedQueries.Interfaces;

// Reads are scoped by role: admins see all queries in their tenant; others see only their own.
// The one exception is FindInTenant, which the delete rule uses to let an admin reach a colleague's record.
public interface ISavedQueryRepository
{
    Task<IReadOnlyList<SavedQuery>> ListForRole(string username, string role, string tenantId, CancellationToken ct = default);
    Task<IReadOnlyList<SavedQuery>> List(string ownerUsername, string tenantId, CancellationToken ct = default);
    Task<SavedQuery?> Find(Guid id, string ownerUsername, string tenantId, CancellationToken ct = default);

    // Tenant-wide lookup, for the one case an admin may reach another user's record.
    Task<SavedQuery?> FindInTenant(Guid id, string tenantId, CancellationToken ct = default);

    Task Add(SavedQuery query, CancellationToken ct = default);
    Task Save(CancellationToken ct = default);
    Task Remove(SavedQuery query, CancellationToken ct = default);
}
