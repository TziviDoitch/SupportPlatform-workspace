using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Repositories;

public sealed class MetadataRepository(SupportPlatformDbContext db) : IMetadataRepository
{
    public async Task<MetadataSnapshot> GetSnapshot(CancellationToken ct = default) =>
        new(
            await db.ReferenceDomains.AsNoTracking().OrderBy(r => r.Code).ToListAsync(ct),
            await db.ReferenceBodyTypes.AsNoTracking().OrderBy(r => r.Code).ToListAsync(ct),
            await db.ReferenceStatuses.AsNoTracking().OrderBy(r => r.Code).ToListAsync(ct),
            await db.ReferenceDistricts.AsNoTracking().OrderBy(r => r.Code).ToListAsync(ct),
            await db.FilterFieldRegistry.AsNoTracking().OrderBy(e => e.SortOrder).ToListAsync(ct));
}
