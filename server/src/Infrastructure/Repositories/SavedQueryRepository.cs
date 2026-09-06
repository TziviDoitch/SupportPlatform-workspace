using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.SavedQueries.Interfaces;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Repositories;

public sealed class SavedQueryRepository(SupportPlatformDbContext db) : ISavedQueryRepository
{
    public async Task<IReadOnlyList<SavedQuery>> ListForRole(
        string username, string role, string tenantId, CancellationToken ct = default)
    {
        var query = db.SavedQueries.Where(q => q.TenantId == tenantId);

        if (!string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
            query = query.Where(q => q.OwnerUsername == username);

        var rows = await query.AsNoTracking().ToListAsync(ct);
        return rows.OrderByDescending(q => q.CreatedAt).ToList();
    }

    public async Task<IReadOnlyList<SavedQuery>> List(
        string ownerUsername, string tenantId, CancellationToken ct = default)
    {
        var rows = await Scoped(ownerUsername, tenantId).AsNoTracking().ToListAsync(ct);
        return rows.OrderByDescending(q => q.CreatedAt).ToList();
    }

    public Task<SavedQuery?> Find(
        Guid id, string ownerUsername, string tenantId, CancellationToken ct = default) =>
        Scoped(ownerUsername, tenantId).FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<SavedQuery?> FindInTenant(Guid id, string tenantId, CancellationToken ct = default) =>
        db.SavedQueries.FirstOrDefaultAsync(q => q.Id == id && q.TenantId == tenantId, ct);

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
