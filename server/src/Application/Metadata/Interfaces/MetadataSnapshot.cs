using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Metadata.Interfaces;

// Raw reference + registry rows, before mapping to the API shape.
public record MetadataSnapshot(
    IReadOnlyList<ReferenceDomain> Domains,
    IReadOnlyList<ReferenceBodyType> BodyTypes,
    IReadOnlyList<ReferenceStatus> Statuses,
    IReadOnlyList<ReferenceDistrict> Districts,
    IReadOnlyList<FilterFieldRegistryEntry> Registry)
{
    // The rows behind a registry entry's referenceList name; empty for an unknown name.
    public IReadOnlyList<ReferenceItem> ReferenceList(string? name) => name switch
    {
        "domains" => Domains,
        "bodyTypes" => BodyTypes,
        "statuses" => Statuses,
        "districts" => Districts,
        _ => []
    };
}
