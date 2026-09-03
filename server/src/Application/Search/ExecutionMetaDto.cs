namespace SupportPlatform.Application.Search;

/// <summary>
/// Timing + cache diagnostics for one search. <see cref="DefinitionHash"/> is a canonical
/// SHA-256 of the definition; <see cref="CacheHit"/> stays <c>false</c> until the cache lands in S5.
/// </summary>
public sealed record ExecutionMetaDto(long DurationMs, int RowCount, bool CacheHit, string DefinitionHash);
