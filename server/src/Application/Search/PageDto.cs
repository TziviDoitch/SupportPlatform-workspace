namespace SupportPlatform.Application.Search;

/// <summary>
/// Echoes <see cref="Paging"/> plus <see cref="TotalGroups"/> — the number of aggregation groups
/// before paging. The search engine returns groups, not raw records: a query with no
/// <c>segmentation</c> yields exactly one group (the overall total).
/// </summary>
public sealed record PageDto(int PageNumber, int PageSize, int TotalGroups);
