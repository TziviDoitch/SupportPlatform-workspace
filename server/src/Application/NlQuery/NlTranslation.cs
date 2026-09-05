using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery;

// What a provider returns: the canonical query it derived (never a value it invented), plus what
// it could not derive. Unresolved is the signal that matters; Confidence is an indication only.
public sealed record NlTranslation(
    QueryDefinition Definition,
    double Confidence,
    IReadOnlyList<string> Unresolved);
