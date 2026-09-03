using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Search;

/// <summary>
/// Everything validation, execution, and question-text rendering need about the shape of the
/// world: the reference lists + filter-field whitelist (<see cref="Snapshot"/>) and the set of
/// known tenant ids.
/// </summary>
public sealed record SearchMetadata(MetadataSnapshot Snapshot, IReadOnlySet<string> TenantIds)
{
    public IReadOnlyList<FilterFieldRegistryEntry> Registry => Snapshot.Registry;

    public FilterFieldRegistryEntry? Field(string id) => Registry.FirstOrDefault(e => e.Id == id);
}
