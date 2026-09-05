namespace SupportPlatform.Infrastructure.Search.Filters;

public sealed record GroupAggregate(object Key, long Count, decimal SumAmountApproved);
