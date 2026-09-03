using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Validation;

namespace SupportPlatform.Application.Tests.Search;

public class QueryDefinitionValidatorTests
{
    private readonly QueryDefinitionValidator _validator = new(TestMetadata.Provider);

    private static QueryDefinition WorkedExample() => new()
    {
        TenantId = "culture-sport-admin",
        Filters = new Dictionary<string, FilterValue>
        {
            ["bodyType"] = new FilterValue.Codes(["association"]),
            ["supportDomain"] = new FilterValue.Codes(["culture"]),
            ["status"] = new FilterValue.Codes(["approved"]),
            ["supportYear"] = new FilterValue.YearRange(2023, 2025)
        },
        Segmentation = ["supportYear"],
        Metrics = ["count"],
        Paging = new Paging(50, 1),
        Sort = [new SortSpec("supportYear", "asc")]
    };

    [Fact]
    public async Task Worked_example_is_valid()
    {
        var result = await _validator.ValidateAsync(WorkedExample());
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public async Task Unknown_filter_field_is_rejected()
    {
        var def = WorkedExample() with
        {
            Filters = new Dictionary<string, FilterValue> { ["costCenter"] = new FilterValue.Codes(["x"]) }
        };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "filters.costCenter");
    }

    [Fact]
    public async Task Reversed_year_range_is_rejected()
    {
        var def = WorkedExample() with
        {
            Filters = new Dictionary<string, FilterValue> { ["supportYear"] = new FilterValue.YearRange(2025, 2023) }
        };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "filters.supportYear");
    }

    [Fact]
    public async Task Non_segmentable_field_in_segmentation_is_rejected()
    {
        var def = WorkedExample() with { Segmentation = ["status"], Sort = [] };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "segmentation[0]");
    }

    [Fact]
    public async Task Unknown_metric_is_rejected()
    {
        var def = WorkedExample() with { Metrics = ["median"], Sort = [] };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "metrics[0]");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public async Task Page_size_outside_1_to_200_is_rejected(int pageSize)
    {
        var def = WorkedExample() with { Paging = new Paging(pageSize, 1) };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "paging.pageSize");
    }

    [Fact]
    public async Task Unknown_tenant_is_rejected()
    {
        var def = WorkedExample() with { TenantId = "ministry-of-magic" };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "tenantId");
    }

    [Fact]
    public async Task Code_list_value_for_a_year_field_is_rejected()
    {
        var def = WorkedExample() with
        {
            Filters = new Dictionary<string, FilterValue> { ["supportYear"] = new FilterValue.Codes(["2024"]) },
            Segmentation = [],
            Sort = []
        };

        var result = await _validator.ValidateAsync(def);

        Assert.Contains(result.Errors, e => e.PropertyName == "filters.supportYear");
    }
}
