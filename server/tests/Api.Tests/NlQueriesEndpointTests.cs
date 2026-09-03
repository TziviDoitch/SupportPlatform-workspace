using System.Net;
using System.Text;
using System.Text.Json;

namespace SupportPlatform.Api.Tests;

/// <summary>
/// <c>POST /api/nl-queries/parse</c> (contract §4): free text in, a reviewable definition out.
/// It never runs the query — the client posts the definition to <c>/api/search</c> after review.
/// </summary>
public class NlQueriesEndpointTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    private async Task<JsonElement> Parse(string body, HttpStatusCode expected)
    {
        var response = await factory.CreateClient().PostAsync("/api/nl-queries/parse", Json(body));
        Assert.Equal(expected, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    [Fact]
    public async Task Returns_a_definition_an_interpretation_and_what_was_not_understood()
    {
        var root = await Parse(
            """{ "text": "כמה עמותות בתחום התרבות אושרו בשנת 2024 לפי מחוז", "tenantId": "culture-sport-admin" }""",
            HttpStatusCode.OK);

        var definition = root.GetProperty("definition");
        Assert.Equal("culture-sport-admin", definition.GetProperty("tenantId").GetString());
        Assert.Equal("culture", definition.GetProperty("filters").GetProperty("supportDomain")[0].GetString());
        Assert.Equal(2024, definition.GetProperty("filters").GetProperty("supportYear").GetProperty("value").GetInt32());
        Assert.Equal("district", definition.GetProperty("segmentation")[0].GetString());

        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("interpretationText").GetString()));
        Assert.InRange(root.GetProperty("confidence").GetDouble(), 0, 1);
        Assert.Equal(0, root.GetProperty("unresolved").GetArrayLength());
    }

    [Fact]
    public async Task Does_not_execute_the_query()
    {
        var root = await Parse("""{ "text": "בקשות בתרבות" }""", HttpStatusCode.OK);

        Assert.False(root.TryGetProperty("rows", out _));
        Assert.False(root.TryGetProperty("executionMeta", out _));
    }

    [Fact]
    public async Task Reports_words_it_could_not_map_instead_of_inventing_filters()
    {
        var root = await Parse("""{ "text": "כמה בקשות הוגשו על ידי אשכולות אזוריים" }""", HttpStatusCode.OK);

        Assert.Empty(root.GetProperty("definition").GetProperty("filters").EnumerateObject());
        Assert.True(root.GetProperty("unresolved").GetArrayLength() > 0);
    }

    [Fact]
    public async Task Empty_text_is_a_validation_problem()
    {
        var root = await Parse("""{ "text": "" }""", HttpStatusCode.BadRequest);

        Assert.True(root.GetProperty("errors").TryGetProperty("text", out _));
    }

    [Fact]
    public async Task A_tenant_that_is_not_the_callers_is_a_403()
    {
        // S8: the caller's tenant is authoritative; naming another in the body is forbidden.
        var root = await Parse(
            """{ "text": "בקשות בתרבות", "tenantId": "welfare-admin" }""", HttpStatusCode.Forbidden);

        Assert.EndsWith("/forbidden", root.GetProperty("type").GetString());
    }
}
