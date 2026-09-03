using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.Tests.Search;

public class QuestionTextRendererTests
{
    private readonly QuestionTextRenderer _renderer = new();

    [Fact]
    public void Renders_the_worked_example_from_the_contract()
    {
        var def = new QueryDefinition
        {
            TenantId = "culture-sport-admin",
            Filters = new Dictionary<string, FilterValue>
            {
                ["bodyType"] = new FilterValue.Codes(["association"]),
                ["supportDomain"] = new FilterValue.Codes(["culture"]),
                ["status"] = new FilterValue.Codes(["approved"]),
                ["supportYear"] = new FilterValue.YearRange(2023, 2025)
            },
            Segmentation = ["supportYear"]
        };

        var text = _renderer.Render(def, TestMetadata.Snapshot);

        Assert.Equal(
            "כמה בקשות תמיכה עם סוג גוף: עמותה, תחום תמיכה: תרבות, סטטוס: מאושר, "
            + "שנת תמיכה: 2023–2025, בפילוח לפי שנת תמיכה?",
            text);
    }

    [Fact]
    public void No_filters_and_no_segmentation_is_the_bare_question()
    {
        var def = new QueryDefinition
        {
            TenantId = "culture-sport-admin",
            Filters = new Dictionary<string, FilterValue>()
        };

        Assert.Equal("כמה בקשות תמיכה?", _renderer.Render(def, TestMetadata.Snapshot));
    }

    [Fact]
    public void Multiple_codes_join_with_or_and_a_single_year_has_no_dash()
    {
        var def = new QueryDefinition
        {
            TenantId = "culture-sport-admin",
            Filters = new Dictionary<string, FilterValue>
            {
                ["district"] = new FilterValue.Codes(["north", "south"]),
                ["supportYear"] = new FilterValue.YearSingle(2024)
            }
        };

        Assert.Equal(
            "כמה בקשות תמיכה עם מחוז: צפון או דרום, שנת תמיכה: 2024?",
            _renderer.Render(def, TestMetadata.Snapshot));
    }

    [Fact]
    public void Segmentation_lists_every_field_label()
    {
        var def = new QueryDefinition
        {
            TenantId = "culture-sport-admin",
            Filters = new Dictionary<string, FilterValue>(),
            Segmentation = ["district", "supportYear"]
        };

        Assert.Equal(
            "כמה בקשות תמיכה, בפילוח לפי מחוז, שנת תמיכה?",
            _renderer.Render(def, TestMetadata.Snapshot));
    }
}
