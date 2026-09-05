using SupportPlatform.Application.NlQuery.RuleBased;

namespace SupportPlatform.Application.NlQuery;

// Which INlQueryProvider serves POST /api/nl-queries/parse. Bound from NlQuery:Provider; the
// value is a provider key, so swapping the AI implementation is configuration, not a recompile.
public sealed class NlQueryOptions
{
    public string Provider { get; init; } = RuleBasedNlQueryProvider.ProviderKey;
}
