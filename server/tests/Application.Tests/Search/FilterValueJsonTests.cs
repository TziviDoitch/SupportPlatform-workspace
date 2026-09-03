using System.Text.Json;
using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.Tests.Search;

public class FilterValueJsonTests
{
    private static readonly JsonSerializerOptions Options =
        new() { Converters = { new FilterValueJsonConverter() } };

    [Fact]
    public void Array_reads_as_a_code_list()
    {
        var value = JsonSerializer.Deserialize<FilterValue>("""["association","company"]""", Options);

        var codes = Assert.IsType<FilterValue.Codes>(value);
        Assert.Equal(["association", "company"], codes.Values);
    }

    [Fact]
    public void Object_with_type_range_reads_as_a_year_range()
    {
        var value = JsonSerializer.Deserialize<FilterValue>("""{"type":"range","from":2023,"to":2025}""", Options);

        var range = Assert.IsType<FilterValue.YearRange>(value);
        Assert.Equal((2023, 2025), (range.From, range.To));
    }

    [Fact]
    public void Object_with_type_single_reads_as_a_single_year()
    {
        var value = JsonSerializer.Deserialize<FilterValue>("""{"type":"single","value":2024}""", Options);

        Assert.Equal(2024, Assert.IsType<FilterValue.YearSingle>(value).Value);
    }

    [Theory]
    [InlineData("\"just a string\"")]
    [InlineData("42")]
    [InlineData("""{"type":"decade","from":2020}""")]
    [InlineData("""{"type":"range","from":2023}""")]
    public void Malformed_values_throw(string json)
    {
        Assert.ThrowsAny<JsonException>(() => JsonSerializer.Deserialize<FilterValue>(json, Options));
    }

    [Fact]
    public void Year_values_round_trip_through_write_then_read()
    {
        FilterValue[] originals = [new FilterValue.YearRange(2023, 2025), new FilterValue.YearSingle(2025)];

        foreach (var original in originals)
        {
            var json = JsonSerializer.Serialize(original, Options);
            Assert.Equal(original, JsonSerializer.Deserialize<FilterValue>(json, Options));
        }
    }

    [Fact]
    public void A_code_list_round_trips_through_write_then_read()
    {
        FilterValue original = new FilterValue.Codes(["north", "south"]);

        var json = JsonSerializer.Serialize(original, Options);
        var back = Assert.IsType<FilterValue.Codes>(JsonSerializer.Deserialize<FilterValue>(json, Options));

        Assert.Equal(["north", "south"], back.Values);
    }
}
