using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.Interfaces;

/// <summary>
/// The AI seam (<c>DESIGN_QA.md</c> §6): free text → the canonical <see cref="QueryDefinition"/>.
/// One responsibility, nothing else — a provider never touches the database, runs a search, or
/// validates; <see cref="NlQueryService"/> does those with the existing S2 pieces.
///
/// <c>Translate</c>, not "parse": parsing is how the PoC's
/// <see cref="RuleBased.RuleBasedNlQueryProvider"/> happens to work, while an LLM-backed provider
/// would translate some other way. The endpoint and use case keep the contract's <c>parse</c>
/// wording (<c>api-contract.md</c> §4) — <c>API Parse → NlQueryService → provider Translate</c>.
///
/// Implementations register under a key in <c>AddApplication</c> and are selected at runtime by
/// <c>NlQuery:Provider</c>, so swapping the AI is configuration rather than a recompile.
/// </summary>
public interface INlQueryProvider
{
    /// <param name="metadata">
    /// The vocabulary to translate against, passed in rather than fetched, so a provider stays
    /// free of data access.
    /// </param>
    Task<NlTranslation> Translate(
        string text, string tenantId, SearchMetadata metadata, CancellationToken ct = default);
}
