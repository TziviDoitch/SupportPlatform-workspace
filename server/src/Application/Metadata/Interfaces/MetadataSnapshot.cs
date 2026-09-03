using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Metadata.Interfaces;

/// <summary>Raw reference + registry rows, before mapping to the API shape.</summary>
public record MetadataSnapshot(
    IReadOnlyList<ReferenceDomain> Domains,
    IReadOnlyList<ReferenceBodyType> BodyTypes,
    IReadOnlyList<ReferenceStatus> Statuses,
    IReadOnlyList<ReferenceDistrict> Districts,
    IReadOnlyList<FilterFieldRegistryEntry> Registry)
{
    /// <summary>
    /// The rows behind a registry entry's <c>referenceList</c> name; empty for an unknown name.
    /// The one place that maps list name → rows.
    /// </summary>
    public IReadOnlyList<ReferenceItem> ReferenceList(string? name) => name switch
    {
        "domains" => Domains,
        "bodyTypes" => BodyTypes,
        "statuses" => Statuses,
        "districts" => Districts,
        _ => []
    };
}
