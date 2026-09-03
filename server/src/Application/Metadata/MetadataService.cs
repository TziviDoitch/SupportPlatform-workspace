using SupportPlatform.Application.Identity;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Metadata;

/// <summary>
/// Builds the <see cref="MetadataResponse"/> — reference lists + filter-field registry — for the
/// caller's tenant. The requested <c>tenantId</c> is validated against identity, not trusted (S8).
/// </summary>
public sealed class MetadataService(IMetadataRepository repository, TenantAccessGuard tenantAccess)
    : IMetadataService
{
    public async Task<MetadataResponse> Get(string tenantId, CancellationToken ct = default)
    {
        // The authenticated caller's tenant is authoritative (S8): a request for another tenant's
        // metadata is a 403, not a silent scope switch (docs/ARCHITECTURE.md §8.1).
        tenantId = tenantAccess.EnsureTenant(tenantId);

        var snapshot = await repository.GetSnapshot(ct);

        var references = new ReferencesDto(
            Map(snapshot.Domains),
            Map(snapshot.BodyTypes),
            Map(snapshot.Statuses),
            Map(snapshot.Districts));

        var registry = snapshot.Registry
            .Select(e => new FilterFieldDto(e.Id, e.Label, e.Kind, e.ReferenceList, e.Operators, e.Segmentable))
            .ToList();

        return new MetadataResponse(tenantId, references, registry);
    }

    private static IReadOnlyList<ReferenceItemDto> Map(IEnumerable<ReferenceItem> items) =>
        items.Select(i => new ReferenceItemDto(i.Code, i.Label)).ToList();
}
