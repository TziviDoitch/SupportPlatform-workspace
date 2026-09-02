using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Metadata.Interfaces;

/// <summary>Raw reference + registry rows, before mapping to the API shape.</summary>
public record MetadataSnapshot(
    IReadOnlyList<ReferenceDomain> Domains,
    IReadOnlyList<ReferenceBodyType> BodyTypes,
    IReadOnlyList<ReferenceStatus> Statuses,
    IReadOnlyList<ReferenceDistrict> Districts,
    IReadOnlyList<FilterFieldRegistryEntry> Registry);
