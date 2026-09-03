namespace SupportPlatform.Infrastructure.Search.Filters;

/// <summary>One database group produced by <see cref="FilterHandler.GroupAggregate"/>:
/// the scalar key value plus the count and approved-amount sum of the rows in it.</summary>
public sealed record GroupAggregate(object Key, long Count, decimal SumAmountApproved);
