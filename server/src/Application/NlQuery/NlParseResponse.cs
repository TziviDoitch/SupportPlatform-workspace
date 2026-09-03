using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery;

/// <summary>
/// Response of <c>POST /api/nl-queries/parse</c> (<c>api-contract.md</c> §4). Parsing never runs
/// the query — the client reviews the interpretation and posts the definition to
/// <c>POST /api/search</c> itself.
/// </summary>
/// <param name="Definition">A validated <see cref="QueryDefinition"/> the client can run or save.</param>
/// <param name="InterpretationText">Hebrew read-back, from the same renderer <c>/api/search</c> uses.</param>
/// <param name="Confidence">0..1 — share of the meaningful words that were understood.</param>
/// <param name="Unresolved">Words that could not be mapped to any filter or segmentation.</param>
public record NlParseResponse(
    QueryDefinition Definition,
    string InterpretationText,
    double Confidence,
    IReadOnlyList<string> Unresolved);
