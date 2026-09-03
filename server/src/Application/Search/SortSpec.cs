namespace SupportPlatform.Application.Search;

/// <summary>
/// One ordering key. <see cref="Field"/> is a segmentation field id or a metric name
/// (<c>count</c> / <c>sumAmountApproved</c>); <see cref="Direction"/> is <c>asc</c> or <c>desc</c>.
/// </summary>
public sealed record SortSpec(string Field, string Direction);
