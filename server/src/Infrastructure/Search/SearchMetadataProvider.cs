using SupportPlatform.Application.Common.Interfaces;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search;

/// <summary>
/// Loads the reference/registry snapshot and the known tenant ids once per request (the type is
/// scoped, so the memoized value is per request).
/// </summary>
public sealed class SearchMetadataProvider(IMetadataRepository metadata, IRepository<Tenant> tenants)
    : ISearchMetadataProvider
{
    private SearchMetadata? _cached;

    public async Task<SearchMetadata> Get(CancellationToken ct = default)
    {
        if (_cached is not null)
            return _cached;

        var snapshot = await metadata.GetSnapshot(ct);
        var tenantIds = (await tenants.ListAllAsync(ct)).Select(t => t.Id).ToHashSet();

        return _cached = new SearchMetadata(snapshot, tenantIds);
    }
}
