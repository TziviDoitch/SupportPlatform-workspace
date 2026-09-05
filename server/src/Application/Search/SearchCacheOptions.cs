namespace SupportPlatform.Application.Search;

// TTL for the search result cache (dedup). Bound from Search:CacheTtlSeconds; 0 or less turns
// dedup off entirely.
public sealed class SearchCacheOptions
{
    public int TtlSeconds { get; init; } = 60;

    public TimeSpan Ttl => TimeSpan.FromSeconds(TtlSeconds);
}
