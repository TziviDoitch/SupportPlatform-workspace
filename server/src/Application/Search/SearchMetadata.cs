using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Search;

// The reference lists + filter-field whitelist plus the set of known tenant ids — what
// validation, execution and question-text rendering need.
public sealed record SearchMetadata(MetadataSnapshot Snapshot, IReadOnlySet<string> TenantIds)
{
    public IReadOnlyList<FilterFieldRegistryEntry> Registry => Snapshot.Registry;

    public FilterFieldRegistryEntry? Field(string id) => Registry.FirstOrDefault(e => e.Id == id);
}
