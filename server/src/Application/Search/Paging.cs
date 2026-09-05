namespace SupportPlatform.Application.Search;

public sealed record Paging(int PageSize, int PageNumber)
{
    public static readonly Paging Default = new(50, 1);
}
