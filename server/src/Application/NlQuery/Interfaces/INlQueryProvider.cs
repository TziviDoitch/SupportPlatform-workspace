using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.Interfaces;

// The AI seam: free text -> canonical QueryDefinition, and nothing else. A provider never touches
// the database, runs a search, or validates — NlQueryService does those. Implementations register
// under a key and are selected at runtime by NlQuery:Provider.
public interface INlQueryProvider
{
    // metadata is passed in rather than fetched so a provider stays free of data access.
    Task<NlTranslation> Translate(
        string text, string tenantId, SearchMetadata metadata, CancellationToken ct = default);
}
