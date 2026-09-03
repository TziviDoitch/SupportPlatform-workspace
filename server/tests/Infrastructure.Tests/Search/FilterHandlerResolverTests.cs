using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Search.Filters;

namespace SupportPlatform.Infrastructure.Tests.Search;

public class FilterHandlerResolverTests
{
    private readonly FilterHandlerResolver _resolver = new(FilterHandlers.Default);

    private static FilterFieldRegistryEntry Entry(string id, string kind) => new()
    {
        Id = id, Label = id, Kind = kind, Operators = [], Segmentable = true
    };

    [Fact]
    public void Resolves_a_code_list_field_to_the_code_list_handler()
    {
        var handler = _resolver.Resolve(Entry("status", "codeList"));

        Assert.IsType<CodeListFilterHandler>(handler);
        Assert.Equal("status", handler.FieldId);
    }

    [Fact]
    public void Resolves_the_year_field_to_the_year_range_handler()
    {
        Assert.IsType<YearRangeFilterHandler>(_resolver.Resolve(Entry("supportYear", "yearRange")));
    }

    [Fact]
    public void An_unregistered_field_throws()
    {
        Assert.Throws<InvalidOperationException>(() => _resolver.Resolve(Entry("costCenter", "codeList")));
    }

    [Fact]
    public void A_kind_that_disagrees_with_the_handler_throws()
    {
        Assert.Throws<InvalidOperationException>(() => _resolver.Resolve(Entry("status", "yearRange")));
    }
}
