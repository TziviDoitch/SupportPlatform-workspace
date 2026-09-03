using SupportPlatform.Application.NlQuery.RuleBased;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Tests.Search;

namespace SupportPlatform.Application.Tests.NlQuery;

/// <summary>
/// The parser is the whole of S6's "AI": deterministic, metadata-driven, and never inventing a
/// value it did not read in the question.
/// </summary>
public class RuleBasedNlQueryProviderTests
{
    private const string Tenant = "culture-sport-admin";
    private readonly RuleBasedNlQueryProvider _provider = new();

    private async Task<(QueryDefinition Definition, double Confidence, IReadOnlyList<string> Unresolved)> Parse(
        string text)
    {
        var result = await _provider.Translate(text, Tenant, TestMetadata.SearchMetadata);
        return (result.Definition, result.Confidence, result.Unresolved);
    }

    private static IReadOnlyList<string> Codes(QueryDefinition def, string field) =>
        Assert.IsType<FilterValue.Codes>(def.Filters[field]).Values;

    [Fact]
    public async Task Translates_the_contract_example_into_the_expected_definition()
    {
        var (def, _, unresolved) = await Parse("כמה עמותות בתחום התרבות אושרו בשנת 2024");

        Assert.Equal(Tenant, def.TenantId);
        Assert.Equal(["association"], Codes(def, "bodyType"));
        Assert.Equal(["culture"], Codes(def, "supportDomain"));
        Assert.Equal(["approved"], Codes(def, "status"));
        Assert.Equal(new FilterValue.YearSingle(2024), def.Filters["supportYear"]);
        Assert.Empty(def.Segmentation);
        Assert.Empty(unresolved);
    }

    [Fact]
    public async Task Reads_a_single_year()
    {
        var (def, _, _) = await Parse("בקשות תמיכה בשנת 2025");

        Assert.Equal(new FilterValue.YearSingle(2025), def.Filters["supportYear"]);
    }

    [Fact]
    public async Task Reads_two_years_as_the_range_they_span()
    {
        var (def, _, _) = await Parse("בקשות תמיכה בין 2023 ל-2025");

        Assert.Equal(new FilterValue.YearRange(2023, 2025), def.Filters["supportYear"]);
    }

    [Theory]
    [InlineData("בקשות מעמותות", "bodyType", "association")]
    [InlineData("בקשות בספורט", "supportDomain", "sport")]
    [InlineData("בקשות שנדחו", "status", "rejected")]
    [InlineData("בקשות במחוז דרום", "district", "south")]
    public async Task Matches_reference_values_by_their_metadata_label(string text, string field, string code)
    {
        var (def, _, _) = await Parse(text);

        Assert.Equal([code], Codes(def, field));
    }

    [Fact]
    public async Task Several_values_for_one_field_become_an_in_list()
    {
        var (def, _, _) = await Parse("בקשות בתרבות ובספורט");

        Assert.Equal(["culture", "sport"], Codes(def, "supportDomain"));
    }

    [Fact]
    public async Task Reads_segmentation_after_a_grouping_marker()
    {
        var (def, _, _) = await Parse("כמה בקשות תמיכה לפי מחוז");

        Assert.Equal(["district"], def.Segmentation);
    }

    [Fact]
    public async Task Reads_several_segmentation_fields()
    {
        var (def, _, _) = await Parse("בקשות תמיכה בפילוח לפי מחוז ושנה");

        Assert.Equal(["district", "supportYear"], def.Segmentation);
    }

    [Fact]
    public async Task A_value_before_the_grouping_marker_stays_a_filter()
    {
        var (def, _, _) = await Parse("בקשות בתחום התרבות לפי מחוז");

        Assert.Equal(["culture"], Codes(def, "supportDomain"));
        Assert.Equal(["district"], def.Segmentation);
    }

    [Fact]
    public async Task An_ambiguous_grouping_word_resolves_to_nothing()
    {
        // "תמיכה" belongs to both "תחום תמיכה" and "שנת תמיכה" — matching either would be a guess.
        var (def, _, _) = await Parse("בקשות לפי תמיכה");

        Assert.Empty(def.Segmentation);
    }

    [Fact]
    public async Task A_grouping_it_could_not_apply_is_reported_rather_than_dropped_silently()
    {
        // status is not segmentable, so the grouping cannot be honoured — say so.
        var (def, confidence, unresolved) = await Parse("כמה בקשות לפי סטטוס");

        Assert.Empty(def.Segmentation);
        Assert.Contains("סטטוס", unresolved);
        Assert.True(confidence < 1);
    }

    [Fact]
    public async Task A_four_digit_number_that_is_not_a_year_does_not_become_a_year_filter()
    {
        var (def, _, unresolved) = await Parse("בקשות בתרבות מעל 5000 שקל");

        Assert.False(def.Filters.ContainsKey("supportYear"));
        Assert.Contains("5000", unresolved);
    }

    [Fact]
    public async Task Unrecognised_words_are_reported_and_no_value_is_invented()
    {
        var (def, confidence, unresolved) = await Parse("כמה בקשות הוגשו על ידי אשכולות אזוריים");

        Assert.Empty(def.Filters);
        Assert.Empty(def.Segmentation);
        Assert.Contains("אשכולות", unresolved);
        Assert.True(confidence < 1);
    }

    [Fact]
    public async Task Text_with_nothing_to_understand_scores_zero()
    {
        var (def, confidence, _) = await Parse("כמה יש");

        Assert.Empty(def.Filters);
        Assert.Equal(0, confidence);
    }

    [Fact]
    public async Task Is_deterministic()
    {
        const string text = "כמה עמותות בתחום התרבות אושרו בשנת 2024 לפי מחוז";

        var first = await Parse(text);
        var second = await Parse(text);

        // Records compare their collection members by reference — the canonical hash is the real test.
        Assert.Equal(DefinitionHasher.Hash(first.Definition), DefinitionHasher.Hash(second.Definition));
        Assert.Equal(first.Confidence, second.Confidence);
    }

    [Fact]
    public async Task Always_requests_both_contract_metrics_and_the_default_page()
    {
        var (def, _, _) = await Parse("בקשות בתרבות");

        Assert.Equal([Metric.Count, Metric.SumAmountApproved], def.Metrics);
        Assert.Equal(Paging.Default, def.Paging);
    }
}
