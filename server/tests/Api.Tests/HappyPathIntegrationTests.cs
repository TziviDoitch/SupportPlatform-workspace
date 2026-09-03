using System.Net;
using System.Text;
using System.Text.Json;

namespace SupportPlatform.Api.Tests;

/// <summary>
/// The one end-to-end happy path (plan §6 S9): a single caller walks the whole chain over HTTP —
/// identity ⇒ metadata ⇒ search ⇒ save ⇒ run ⇒ NL parse — and each hop feeds the next. The
/// per-endpoint edge cases live in the other <c>*EndpointTests</c>; this test only proves the
/// pieces compose.
/// </summary>
public class HappyPathIntegrationTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private const string Tenant = "culture-sport-admin";

    [Fact]
    public async Task Identity_metadata_search_save_run_and_parse_compose()
    {
        // 1. Identity — the PoC login seam is the X-User header (no JWT; server/CLAUDE.md).
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-User", "sarah"); // seeded analyst in culture-sport-admin

        // 2. Metadata — the form's source of truth. Take a real field id and a real code from it.
        var metadata = await GetJson(client, $"/api/metadata?tenantId={Tenant}");
        Assert.Equal(Tenant, metadata.GetProperty("tenantId").GetString());

        var registryIds = metadata.GetProperty("filterFieldRegistry").EnumerateArray()
            .Select(e => e.GetProperty("id").GetString()).ToList();
        Assert.Contains("supportDomain", registryIds);
        Assert.Contains("supportYear", registryIds);

        var domainCode = metadata.GetProperty("references").GetProperty("domains")[0].GetProperty("code").GetString();

        // 3. Search — a definition built from what metadata returned.
        var definition = $$"""
        {
          "tenantId": "{{Tenant}}",
          "filters": { "supportDomain": ["{{domainCode}}"] },
          "segmentation": ["supportYear"],
          "metrics": ["count"],
          "paging": { "pageSize": 50, "pageNumber": 1 }
        }
        """;

        var search = await PostJson(client, "/api/search", definition, HttpStatusCode.OK);
        var questionText = search.GetProperty("questionText").GetString();
        var searchHash = search.GetProperty("executionMeta").GetProperty("definitionHash").GetString();
        Assert.False(string.IsNullOrWhiteSpace(questionText));
        Assert.StartsWith("sha256:", searchHash);
        Assert.True(search.GetProperty("aggregations").GetArrayLength() > 0);

        // 4. Save — the same definition, stored and scoped to the caller.
        var created = await PostJson(
            client, "/api/saved-queries",
            $$"""{ "name": "culture by year", "definition": {{definition}} }""",
            HttpStatusCode.Created);
        var savedId = created.GetProperty("id").GetString();
        Assert.Equal("sarah", created.GetProperty("ownerUsername").GetString());
        Assert.Equal(Tenant, created.GetProperty("tenantId").GetString());

        // 5. Run — re-runs the stored definition; same hash and question as the direct search.
        var run = await PostJson(client, $"/api/saved-queries/{savedId}/run", body: null, HttpStatusCode.OK);
        Assert.Equal(questionText, run.GetProperty("questionText").GetString());
        Assert.Equal(searchHash, run.GetProperty("executionMeta").GetProperty("definitionHash").GetString());

        var afterRun = await GetJson(client, $"/api/saved-queries/{savedId}");
        Assert.NotEqual(JsonValueKind.Null, afterRun.GetProperty("lastRunAt").ValueKind);

        // 6. NL parse — free text ⇒ a reviewable definition for the same tenant. Never executes.
        var parsed = await PostJson(
            client, "/api/nl-queries/parse",
            $$"""{ "text": "כמה עמותות בתחום התרבות אושרו בשנת 2024 לפי מחוז", "tenantId": "{{Tenant}}" }""",
            HttpStatusCode.OK);
        Assert.Equal(Tenant, parsed.GetProperty("definition").GetProperty("tenantId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(parsed.GetProperty("interpretationText").GetString()));
        Assert.False(parsed.TryGetProperty("executionMeta", out _));
    }

    private static async Task<JsonElement> GetJson(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    private static async Task<JsonElement> PostJson(
        HttpClient client, string url, string? body, HttpStatusCode expected)
    {
        var content = body is null ? null : new StringContent(body, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        Assert.Equal(expected, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }
}
