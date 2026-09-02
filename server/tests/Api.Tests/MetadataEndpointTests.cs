using System.Net;
using System.Text.Json;

namespace SupportPlatform.Api.Tests;

public class MetadataEndpointTests(TestApiFactory factory) : IClassFixture<TestApiFactory>
{
    [Fact]
    public async Task Returns_references_and_registry_for_a_tenant()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/metadata?tenantId=culture-sport-admin");
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        Assert.Equal("culture-sport-admin", root.GetProperty("tenantId").GetString());

        var references = root.GetProperty("references");
        Assert.Equal(2, references.GetProperty("domains").GetArrayLength());
        Assert.Equal(2, references.GetProperty("bodyTypes").GetArrayLength());
        Assert.Equal(3, references.GetProperty("statuses").GetArrayLength());
        Assert.Equal(3, references.GetProperty("districts").GetArrayLength());

        var domainCodes = references.GetProperty("domains").EnumerateArray()
            .Select(d => d.GetProperty("code").GetString()).ToList();
        Assert.Contains("culture", domainCodes);

        var registry = root.GetProperty("filterFieldRegistry");
        Assert.Equal(5, registry.GetArrayLength());

        var ids = registry.EnumerateArray().Select(e => e.GetProperty("id").GetString()).ToList();
        Assert.Equal(["bodyType", "supportDomain", "status", "district", "supportYear"], ids);

        var year = registry.EnumerateArray().Single(e => e.GetProperty("id").GetString() == "supportYear");
        Assert.Equal("yearRange", year.GetProperty("kind").GetString());
        Assert.Equal(
            ["range", "single"],
            year.GetProperty("operators").EnumerateArray().Select(o => o.GetString()));
    }

    [Fact]
    public async Task Missing_tenantId_is_a_400()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/metadata");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
