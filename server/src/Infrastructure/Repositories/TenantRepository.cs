using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Common.Interfaces;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Repositories;

/// <summary>
/// The known tenants — read whole to validate <c>QueryDefinition.TenantId</c> against the
/// whitelist (<see cref="Search.SearchMetadataProvider"/>).
/// </summary>
public sealed class TenantRepository(SupportPlatformDbContext db) : IRepository<Tenant>
{
    public async Task<IReadOnlyList<Tenant>> ListAllAsync(CancellationToken ct = default) =>
        await db.Tenants.AsNoTracking().ToListAsync(ct);
}
