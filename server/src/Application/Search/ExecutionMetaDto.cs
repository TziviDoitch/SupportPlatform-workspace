namespace SupportPlatform.Application.Search;

public sealed record ExecutionMetaDto(long DurationMs, int RowCount, bool CacheHit, string DefinitionHash);
