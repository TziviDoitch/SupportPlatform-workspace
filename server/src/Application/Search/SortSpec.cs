namespace SupportPlatform.Application.Search;

// Field is a segmentation field id or a metric name; Direction is "asc" or "desc".
public sealed record SortSpec(string Field, string Direction);
