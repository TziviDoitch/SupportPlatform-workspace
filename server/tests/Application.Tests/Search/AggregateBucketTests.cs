using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.Tests.Search;

public class AggregateBucketTests
{
    private static readonly AggregateBucket Bucket =
        new(new Dictionary<string, object> { ["supportYear"] = 2024 }, Count: 12, SumAmountApproved: 5000m);

    [Fact]
    public void A_known_metric_returns_its_value()
    {
        Assert.Equal(12L, Bucket.Value(Metric.Count));
        Assert.Equal(5000m, Bucket.Value(Metric.SumAmountApproved));
    }

    [Fact]
    public void Every_declared_metric_can_be_read()
    {
        // Guards the one way a new metric goes wrong: added to Metric.All and forgotten in Value().
        foreach (var metric in Metric.All)
            Assert.NotNull(Bucket.Value(metric));
    }

    [Fact]
    public void An_unknown_metric_throws_a_named_argument_error()
    {
        var ex = Assert.Throws<ArgumentOutOfRangeException>(() => Bucket.Value("avgAmountApproved"));

        Assert.Equal("metric", ex.ParamName);
        Assert.Contains("avgAmountApproved", ex.Message);
    }
}
