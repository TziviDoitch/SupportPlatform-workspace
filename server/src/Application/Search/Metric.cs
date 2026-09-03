namespace SupportPlatform.Application.Search;

/// <summary>The metric names allowed by the API contract.</summary>
public static class Metric
{
    public const string Count = "count";
    public const string SumAmountApproved = "sumAmountApproved";

    public static readonly IReadOnlySet<string> All = new HashSet<string> { Count, SumAmountApproved };
}
