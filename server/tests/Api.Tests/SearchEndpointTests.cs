using System.Net;
using System.Text;
using System.Text.Json;

namespace SupportPlatform.Api.Tests;

public class SearchEndpointTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private const string WorkedExample = """
    {
      "tenantId": "culture-sport-admin",
      "filters": {
        "bodyType": ["association"],
        "supportDomain": ["culture"],
        "status": ["approved"],
        "supportYear": { "type": "range", "from": 2023, "to": 2025 }
      },
      "segmentation": ["supportYear"],
      "metrics": ["count"],
      "paging": { "pageSize": 50, "pageNumber": 1 },
      "sort": [ { "field": "supportYear", "direction": "asc" } ]
    }
    """;

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    private async Task<JsonElement> Post(string body, HttpStatusCode expected)
    {
        var response = await factory.CreateClient().PostAsync("/api/search", Json(body));
        Assert.Equal(expected, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    [Fact]
    public async Task Worked_example_returns_rows_aggregations_and_question_text()
    {
        var root = await Post(WorkedExample, HttpStatusCode.OK);

        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("questionText").GetString()));
        Assert.True(root.GetProperty("aggregations").GetArrayLength() > 0);
        Assert.True(root.GetProperty("rows").GetArrayLength() > 0);
        Assert.True(root.GetProperty("page").GetProperty("totalRows").GetInt32() > 0);

        var meta = root.GetProperty("executionMeta");
        Assert.StartsWith("sha256:", meta.GetProperty("definitionHash").GetString());
        Assert.False(meta.GetProperty("cacheHit").GetBoolean());

        foreach (var agg in root.GetProperty("aggregations").EnumerateArray())
        {
            Assert.True(agg.GetProperty("key").TryGetProperty("supportYear", out _));
            Assert.True(agg.GetProperty("metrics").TryGetProperty("count", out _));
        }
    }

    [Fact]
    public async Task Both_metrics_are_returned_when_requested()
    {
        var body = WorkedExample.Replace("\"metrics\": [\"count\"]", "\"metrics\": [\"count\", \"sumAmountApproved\"]");

        var root = await Post(body, HttpStatusCode.OK);

        var metrics = root.GetProperty("aggregations")[0].GetProperty("metrics");
        Assert.True(metrics.TryGetProperty("count", out _));
        Assert.True(metrics.TryGetProperty("sumAmountApproved", out _));
    }

    [Fact]
    public async Task Unknown_field_id_is_a_problem_details_400()
    {
        var body = WorkedExample.Replace("\"bodyType\": [\"association\"]", "\"costCenter\": [\"x\"]");

        var response = await factory.CreateClient().PostAsync("/api/search", Json(body));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var root = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
        Assert.EndsWith("/validation", root.GetProperty("type").GetString());
        Assert.False(string.IsNullOrWhiteSpace(root.GetProperty("traceId").GetString()));
        Assert.True(root.GetProperty("errors").TryGetProperty("filters.costCenter", out _));
    }

    [Fact]
    public async Task Reversed_year_range_is_a_400_with_a_field_error()
    {
        var body = WorkedExample.Replace("\"from\": 2023, \"to\": 2025", "\"from\": 2025, \"to\": 2023");

        var root = await Post(body, HttpStatusCode.BadRequest);

        Assert.True(root.GetProperty("errors").TryGetProperty("filters.supportYear", out _));
    }

    [Fact]
    public async Task Unknown_tenant_is_a_400()
    {
        var body = WorkedExample.Replace("culture-sport-admin", "ministry-of-magic");

        await Post(body, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_supplied_correlation_id_is_echoed_on_the_response()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/search") { Content = Json(WorkedExample) };
        request.Headers.Add("X-Correlation-Id", "test-correlation-1");

        var response = await factory.CreateClient().SendAsync(request);

        Assert.Equal("test-correlation-1", Assert.Single(response.Headers.GetValues("X-Correlation-Id")));
    }

    [Fact]
    public async Task A_correlation_id_is_generated_when_none_is_supplied()
    {
        var response = await factory.CreateClient().PostAsync("/api/search", Json(WorkedExample));

        Assert.False(string.IsNullOrWhiteSpace(Assert.Single(response.Headers.GetValues("X-Correlation-Id"))));
    }
}
