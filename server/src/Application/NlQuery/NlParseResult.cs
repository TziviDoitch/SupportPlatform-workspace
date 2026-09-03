using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery;

/// <summary>
/// What an <see cref="Interfaces.INlQueryProvider"/> returns: the canonical
/// <see cref="QueryDefinition"/> it derived, plus an honest account of what it could not derive.
/// </summary>
/// <param name="Definition">The canonical query. Never contains a value the provider invented.</param>
/// <param name="Confidence">0..1 — an indication only; <paramref name="Unresolved"/> is the signal that matters.</param>
/// <param name="Unresolved">Words from the question no rule could map. May be empty.</param>
public sealed record NlParseResult(
    QueryDefinition Definition,
    double Confidence,
    IReadOnlyList<string> Unresolved);
