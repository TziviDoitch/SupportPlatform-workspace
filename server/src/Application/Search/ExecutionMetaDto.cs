namespace SupportPlatform.Application.Search;

/// <summary>
/// Timing + cache diagnostics for one search. <see cref="DefinitionHash"/> is a canonical
/// SHA-256 of the definition; <see cref="CacheHit"/> is <c>true</c> when the response was served
/// from the S5 in-memory cache keyed by that hash.
/// </summary>
public sealed record ExecutionMetaDto(long DurationMs, int RowCount, bool CacheHit, string DefinitionHash);
