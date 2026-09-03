using Microsoft.EntityFrameworkCore;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Search;

/// <summary>
/// Loads the reference/registry snapshot and the known tenant ids once per request (the type is
/// scoped, so the memoized value is per request).
/// </summary>
public sealed class SearchMetadataProvider(IMetadataRepository metadata, SupportPlatformDbContext db)
    : ISearchMetadataProvider
{
    private SearchMetadata? _cached;

    public async Task<SearchMetadata> Get(CancellationToken ct = default)
    {
        if (_cached is not null)
            return _cached;

        var snapshot = await metadata.GetSnapshot(ct);
        var tenants = await db.Tenants.AsNoTracking().Select(t => t.Id).ToListAsync(ct);

        return _cached = new SearchMetadata(snapshot, tenants.ToHashSet());
    }
}
