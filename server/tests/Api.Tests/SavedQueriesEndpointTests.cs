using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Api.Tests;

public class SavedQueriesEndpointTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    private const string Definition = """
    {
      "tenantId": "culture-sport-admin",
      "filters": { "status": ["approved"] },
      "segmentation": ["supportYear"],
      "metrics": ["count"],
      "paging": { "pageSize": 50, "pageNumber": 1 }
    }
    """;

    private static string Body(string name) => $$"""{ "name": "{{name}}", "definition": {{Definition}} }""";

    private HttpClient Client(string? user = "sarah")
    {
        var client = factory.CreateClient();
        if (user is not null)
            client.DefaultRequestHeaders.Add("X-User", user);
        return client;
    }

    private static StringContent Json(string body) => new(body, Encoding.UTF8, "application/json");

    private async Task<JsonElement> Create(HttpClient client, string name)
    {
        var response = await client.PostAsync("/api/saved-queries", Json(Body(name)));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
    }

    [Fact]
    public async Task Create_then_list_then_run_updates_last_run()
    {
        var client = Client();
        var created = await Create(client, "approved by year");
        var id = created.GetProperty("id").GetString();

        Assert.Equal("sarah", created.GetProperty("ownerUsername").GetString());
        Assert.Equal("culture-sport-admin", created.GetProperty("tenantId").GetString());

        var list = JsonDocument.Parse(await client.GetStringAsync("/api/saved-queries")).RootElement;
        Assert.Contains(list.EnumerateArray(), e => e.GetProperty("id").GetString() == id);

        var run = await client.PostAsync($"/api/saved-queries/{id}/run", content: null);
        Assert.Equal(HttpStatusCode.OK, run.StatusCode);
        var runBody = JsonDocument.Parse(await run.Content.ReadAsStringAsync()).RootElement;
        Assert.False(string.IsNullOrWhiteSpace(runBody.GetProperty("questionText").GetString()));
        Assert.StartsWith("sha256:", runBody.GetProperty("executionMeta").GetProperty("definitionHash").GetString());

        var after = JsonDocument.Parse(await client.GetStringAsync($"/api/saved-queries/{id}")).RootElement;
        Assert.NotEqual(JsonValueKind.Null, after.GetProperty("lastRunAt").ValueKind);
        Assert.True(after.GetProperty("lastRunRowCount").GetInt32() >= 0);
    }

    [Fact]
    public async Task Delete_removes_the_record()
    {
        var client = Client();
        var id = (await Create(client, "to delete")).GetProperty("id").GetString();

        var deleted = await client.DeleteAsync($"/api/saved-queries/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);

        var get = await client.GetAsync($"/api/saved-queries/{id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task Another_users_saved_query_is_a_404()
    {
        var id = (await Create(Client("sarah"), "sarah private")).GetProperty("id").GetString();

        var asMichal = await Client("michal").GetAsync($"/api/saved-queries/{id}");

        Assert.Equal(HttpStatusCode.NotFound, asMichal.StatusCode);
    }

    [Fact]
    public async Task A_blank_name_is_a_problem_details_400()
    {
        var response = await Client().PostAsync("/api/saved-queries", Json(Body("  ")));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Mutations_are_written_to_the_audit_log()
    {
        var id = (await Create(Client(), "audited")).GetProperty("id").GetString();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportPlatformDbContext>();

        Assert.Contains(
            db.AuditLogs.ToList(),
            a => a.Action == "create" && a.EntityType == "SavedQuery" && a.EntityId == id && a.User == "sarah");
    }
}
