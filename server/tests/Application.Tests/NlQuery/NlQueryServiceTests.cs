using FluentValidation;
using SupportPlatform.Application.Common;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.NlQuery;
using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.NlQuery.RuleBased;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Validation;
using SupportPlatform.Application.Tests.Search;

namespace SupportPlatform.Application.Tests.NlQuery;

/// <summary>
/// The service owns everything around the AI seam: validation, the Hebrew read-back, and audit.
/// Parsing must never run a search.
/// </summary>
public class NlQueryServiceTests
{
    private readonly RecordingAuditService _audit = new();

    private NlQueryService Service(INlQueryProvider? provider = null) => new(
        provider ?? new RuleBasedNlQueryProvider(),
        TestMetadata.Provider,
        new QueryDefinitionValidator(TestMetadata.Provider),
        new QuestionTextRenderer(),
        new TenantAccessGuard(new FakeCurrentUser()),
        _audit);

    [Fact]
    public async Task Returns_the_definition_the_provider_produced_with_a_hebrew_read_back()
    {
        var response = await Service().Parse(new NlParseRequest("כמה עמותות בתחום התרבות אושרו בשנת 2024", null));

        Assert.Equal(["association"], Assert.IsType<FilterValue.Codes>(response.Definition.Filters["bodyType"]).Values);
        Assert.StartsWith("כמה בקשות תמיכה", response.InterpretationText);
        Assert.Contains("תרבות", response.InterpretationText);
        Assert.Empty(response.Unresolved);
    }

    [Fact]
    public async Task Falls_back_to_the_callers_tenant_when_none_is_given()
    {
        var response = await Service().Parse(new NlParseRequest("בקשות בתרבות", null));

        Assert.Equal("culture-sport-admin", response.Definition.TenantId);
    }

    [Fact]
    public async Task A_tenant_that_is_not_the_callers_is_forbidden()
    {
        await Assert.ThrowsAsync<ForbiddenException>(
            () => Service().Parse(new NlParseRequest("בקשות בתרבות", "welfare-admin")));
    }

    [Fact]
    public async Task Rejects_empty_text()
    {
        var request = new NlParseRequest("   ", null);

        var error = await Assert.ThrowsAsync<ValidationException>(() => Service().Parse(request));

        Assert.Contains(error.Errors, e => e.PropertyName == "text");
        Assert.Empty(_audit.Records);
    }

    [Fact]
    public async Task Rejects_a_definition_the_provider_produced_that_the_whitelist_refuses()
    {
        var service = Service(new StubProvider(new QueryDefinition
        {
            TenantId = "culture-sport-admin",
            Filters = new Dictionary<string, FilterValue> { ["madeUpField"] = new FilterValue.Codes(["x"]) }
        }));

        var error = await Assert.ThrowsAsync<ValidationException>(
            () => service.Parse(new NlParseRequest("שאלה", null)));

        Assert.Contains(error.Errors, e => e.PropertyName == "filters.madeUpField");
    }

    [Fact]
    public async Task Records_the_parse_in_the_audit_log()
    {
        await Service().Parse(new NlParseRequest("בקשות בתרבות", null));

        Assert.Contains(("nl-parse", "QueryDefinition", (string?)null), _audit.Records);
    }

    /// <summary>A provider that returns a fixed definition — the seam under test, not the parser.</summary>
    private sealed class StubProvider(QueryDefinition definition) : INlQueryProvider
    {
        public Task<NlTranslation> Translate(
            string text, string tenantId, SearchMetadata metadata, CancellationToken ct = default) =>
            Task.FromResult(new NlTranslation(definition, 1, []));
    }
}
