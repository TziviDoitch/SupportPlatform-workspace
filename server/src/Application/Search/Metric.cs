namespace SupportPlatform.Application.Search;

public static class Metric
{
    public const string Count = "count";
    public const string SumAmountApproved = "sumAmountApproved";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Count, SumAmountApproved };
}
