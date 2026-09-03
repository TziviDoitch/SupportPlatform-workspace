using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.Tests.Search;

public class DefinitionHasherTests
{
    private static QueryDefinition Def(IReadOnlyDictionary<string, FilterValue> filters) => new()
    {
        TenantId = "culture-sport-admin",
        Filters = filters,
        Segmentation = ["supportYear"],
        Metrics = ["count"]
    };

    [Fact]
    public void Hash_is_prefixed_and_stable()
    {
        var def = Def(new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["approved"]) });

        var first = DefinitionHasher.Hash(def);

        Assert.StartsWith("sha256:", first);
        Assert.Equal(first, DefinitionHasher.Hash(def));
    }

    [Fact]
    public void Filter_key_order_does_not_change_the_hash()
    {
        var a = Def(new Dictionary<string, FilterValue>
        {
            ["status"] = new FilterValue.Codes(["approved"]),
            ["bodyType"] = new FilterValue.Codes(["association"])
        });
        var b = Def(new Dictionary<string, FilterValue>
        {
            ["bodyType"] = new FilterValue.Codes(["association"]),
            ["status"] = new FilterValue.Codes(["approved"])
        });

        Assert.Equal(DefinitionHasher.Hash(a), DefinitionHasher.Hash(b));
    }

    [Fact]
    public void Code_order_within_a_filter_does_not_change_the_hash()
    {
        var a = Def(new Dictionary<string, FilterValue>
        {
            ["status"] = new FilterValue.Codes(["approved", "pending"])
        });
        var b = Def(new Dictionary<string, FilterValue>
        {
            ["status"] = new FilterValue.Codes(["pending", "approved"])
        });

        Assert.Equal(DefinitionHasher.Hash(a), DefinitionHasher.Hash(b));
    }

    [Fact]
    public void Metric_order_does_not_change_the_hash()
    {
        var filters = new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["approved"]) };

        var a = Def(filters) with { Metrics = ["count", "sumAmountApproved"] };
        var b = Def(filters) with { Metrics = ["sumAmountApproved", "count"] };

        Assert.Equal(DefinitionHasher.Hash(a), DefinitionHasher.Hash(b));
    }

    [Fact]
    public void Different_definitions_hash_differently()
    {
        var a = Def(new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["approved"]) });
        var b = Def(new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["pending"]) });

        Assert.NotEqual(DefinitionHasher.Hash(a), DefinitionHasher.Hash(b));
    }

    [Fact]
    public void Default_metric_and_explicit_count_hash_the_same()
    {
        var filters = new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["approved"]) };

        var withDefault = Def(filters) with { Metrics = [] };
        var withExplicit = Def(filters) with { Metrics = ["count"] };

        Assert.Equal(DefinitionHasher.Hash(withDefault), DefinitionHasher.Hash(withExplicit));
    }
}
