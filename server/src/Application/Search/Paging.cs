namespace SupportPlatform.Application.Search;

/// <summary>Page window over the result rows. Defaults: 50 rows, page 1.</summary>
public sealed record Paging(int PageSize, int PageNumber)
{
    public static readonly Paging Default = new(50, 1);
}
