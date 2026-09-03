using SupportPlatform.Application.NlQuery.RuleBased;

namespace SupportPlatform.Application.NlQuery;

/// <summary>
/// Which <see cref="Interfaces.INlQueryProvider"/> serves <c>POST /api/nl-queries/parse</c>
/// (<c>DESIGN_QA.md</c> §6). Bound from <c>NlQuery:Provider</c>; the value is a provider key, so
/// swapping the AI implementation is configuration, not a recompile.
/// </summary>
public sealed class NlQueryOptions
{
    public string Provider { get; init; } = RuleBasedNlQueryProvider.ProviderKey;
}
