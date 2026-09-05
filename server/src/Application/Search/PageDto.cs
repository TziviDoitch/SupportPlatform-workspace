namespace SupportPlatform.Application.Search;

// TotalGroups counts aggregation groups, not raw records — a query with no segmentation yields
// one group (the overall total).
public sealed record PageDto(int PageNumber, int PageSize, int TotalGroups);
