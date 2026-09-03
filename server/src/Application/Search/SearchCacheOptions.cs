namespace SupportPlatform.Application.Search;

/// <summary>
/// TTL for the search result cache (dedup — <c>DESIGN_QA.md</c> §5). Bound from
/// <c>Search:CacheTtlSeconds</c>; a short window is enough to absorb a burst of identical runs.
/// A value of <c>0</c> or less turns dedup off entirely (the §7.3 fallback lever).
/// </summary>
public sealed class SearchCacheOptions
{
    public int TtlSeconds { get; init; } = 60;

    public TimeSpan Ttl => TimeSpan.FromSeconds(TtlSeconds);
}
