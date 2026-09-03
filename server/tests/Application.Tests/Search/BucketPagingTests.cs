using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.Tests.Search;

public class BucketPagingTests
{
    private static AggregateBucket Year(int year, long count, decimal sum = 0m) =>
        new(new Dictionary<string, object> { ["supportYear"] = year }, count, sum);

    private static QueryDefinition Def(
        IReadOnlyList<string> segmentation, IReadOnlyList<SortSpec>? sort = null, Paging? paging = null) => new()
    {
        TenantId = "culture-sport-admin",
        Filters = new Dictionary<string, FilterValue>(),
        Segmentation = segmentation,
        Sort = sort ?? [],
        Paging = paging ?? Paging.Default
    };

    [Fact]
    public void Default_order_is_the_segmentation_field_ascending()
    {
        var buckets = new[] { Year(2025, 1), Year(2023, 1), Year(2024, 1) };

        var result = BucketPaging.Apply(buckets, Def(["supportYear"]));

        Assert.Equal(new object[] { 2023, 2024, 2025 }, result.Buckets.Select(b => b.Key["supportYear"]));
        Assert.Equal(3, result.TotalBuckets);
    }

    [Fact]
    public void A_descending_sort_spec_reverses_the_order()
    {
        var buckets = new[] { Year(2023, 1), Year(2024, 1), Year(2025, 1) };

        var result = BucketPaging.Apply(buckets, Def(["supportYear"], [new SortSpec("supportYear", "desc")]));

        Assert.Equal(new object[] { 2025, 2024, 2023 }, result.Buckets.Select(b => b.Key["supportYear"]));
    }

    [Fact]
    public void Sort_can_target_a_metric_name()
    {
        var buckets = new[] { Year(2023, 5), Year(2024, 30), Year(2025, 12) };

        var result = BucketPaging.Apply(buckets, Def(["supportYear"], [new SortSpec("count", "desc")]));

        Assert.Equal(new[] { 30L, 12L, 5L }, result.Buckets.Select(b => b.Count));
    }

    [Fact]
    public void Paging_cuts_the_page_but_reports_the_full_total()
    {
        var buckets = new[] { Year(2023, 1), Year(2024, 1), Year(2025, 1) };

        var result = BucketPaging.Apply(buckets, Def(["supportYear"], paging: new Paging(2, 1)));

        Assert.Equal(2, result.Buckets.Count);
        Assert.Equal(3, result.TotalBuckets);
        Assert.Equal(new object[] { 2023, 2024 }, result.Buckets.Select(b => b.Key["supportYear"]));
    }

    [Fact]
    public void The_second_page_skips_the_first()
    {
        var buckets = new[] { Year(2023, 1), Year(2024, 1), Year(2025, 1) };

        var result = BucketPaging.Apply(buckets, Def(["supportYear"], paging: new Paging(2, 2)));

        Assert.Equal(new object[] { 2025 }, result.Buckets.Select(b => b.Key["supportYear"]));
        Assert.Equal(3, result.TotalBuckets);
    }
}
