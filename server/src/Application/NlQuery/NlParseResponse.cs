using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery;

// Parsing never runs the query — the client reviews the interpretation and posts the definition
// to POST /api/search itself. Confidence is 0..1; Unresolved lists words that mapped to nothing.
public record NlParseResponse(
    QueryDefinition Definition,
    string InterpretationText,
    double Confidence,
    IReadOnlyList<string> Unresolved);
