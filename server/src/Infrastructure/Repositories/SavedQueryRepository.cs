using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.SavedQueries.Interfaces;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Repositories;

/// <summary>EF Core persistence for <see cref="SavedQuery"/>; every read is scoped to owner + tenant.</summary>
public sealed class SavedQueryRepository(SupportPlatformDbContext db) : ISavedQueryRepository
{
    public async Task<IReadOnlyList<SavedQuery>> List(
        string ownerUsername, string tenantId, CancellationToken ct = default)
    {
        // Order client-side: the SQLite test provider can't ORDER BY DateTimeOffset, and a single
        // user's saved-query list is small.
        var rows = await Scoped(ownerUsername, tenantId).AsNoTracking().ToListAsync(ct);
        return rows.OrderByDescending(q => q.CreatedAt).ToList();
    }

    public Task<SavedQuery?> Find(
        Guid id, string ownerUsername, string tenantId, CancellationToken ct = default) =>
        Scoped(ownerUsername, tenantId).FirstOrDefaultAsync(q => q.Id == id, ct);

    public async Task Add(SavedQuery query, CancellationToken ct = default) =>
        await db.SavedQueries.AddAsync(query, ct);

    public Task Save(CancellationToken ct = default) => db.SaveChangesAsync(ct);

    public Task Remove(SavedQuery query, CancellationToken ct = default)
    {
        db.SavedQueries.Remove(query);
        return Task.CompletedTask;
    }

    private IQueryable<SavedQuery> Scoped(string ownerUsername, string tenantId) =>
        db.SavedQueries.Where(q => q.OwnerUsername == ownerUsername && q.TenantId == tenantId);
}
