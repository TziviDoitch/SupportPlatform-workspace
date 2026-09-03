using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.Interfaces;

/// <summary>
/// The AI seam (<c>DESIGN_QA.md</c> Q6): free text → the canonical <see cref="QueryDefinition"/>.
/// One responsibility, nothing else — a provider never touches the database, runs a search, or
/// validates; <see cref="NlQueryService"/> does those with the existing S2 pieces.
///
/// The PoC ships <see cref="RuleBased.RuleBasedNlQueryProvider"/>. Swapping in an LLM-backed
/// provider is a DI registration change and touches no other type.
/// </summary>
public interface INlQueryProvider
{
    /// <param name="metadata">
    /// The vocabulary to translate against, passed in rather than fetched, so a provider stays
    /// free of data access.
    /// </param>
    Task<NlParseResult> Parse(string text, string tenantId, SearchMetadata metadata, CancellationToken ct = default);
}
