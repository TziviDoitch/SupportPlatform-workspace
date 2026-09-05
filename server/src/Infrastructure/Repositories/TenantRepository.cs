using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Common.Interfaces;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Repositories;

// The known tenants — read whole to validate QueryDefinition.TenantId against the whitelist.
public sealed class TenantRepository(SupportPlatformDbContext db) : IRepository<Tenant>
{
    public async Task<IReadOnlyList<Tenant>> ListAllAsync(CancellationToken ct = default) =>
        await db.Tenants.AsNoTracking().ToListAsync(ct);
}
