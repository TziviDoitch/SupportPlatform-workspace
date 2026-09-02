using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Metadata;

public class MetadataService(IMetadataRepository repository) : IMetadataService
{
    public async Task<MetadataResponse> Get(string tenantId, CancellationToken ct = default)
    {
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
