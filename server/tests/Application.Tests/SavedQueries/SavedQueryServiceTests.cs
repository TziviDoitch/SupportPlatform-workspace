using FluentValidation;
using SupportPlatform.Application.Common;
using SupportPlatform.Application.SavedQueries;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Application.Search.Validation;
using SupportPlatform.Application.Tests.Search;

namespace SupportPlatform.Application.Tests.SavedQueries;

public class SavedQueryServiceTests
{
    private readonly FakeSavedQueryRepository _repo = new();
    private readonly RecordingAuditService _audit = new();
    private readonly StubSearch _search = new();
    private FakeCurrentUser _user = new();

    private SavedQueryService Service() => new(
        _repo,
        _user,
        new QueryDefinitionValidator(TestMetadata.Provider),
        _search,
        _audit);

    private static SaveSavedQueryRequest Request(string name = "Approved") => new(
        name,
        new QueryDefinition
        {
            TenantId = "culture-sport-admin",
            Filters = new Dictionary<string, FilterValue> { ["status"] = new FilterValue.Codes(["approved"]) },
            Segmentation = ["supportYear"]
        });

    [Fact]
    public async Task Create_stamps_owner_tenant_and_hash_and_audits()
    {
        var dto = await Service().Create(Request());

        Assert.Equal("sarah", dto.OwnerUsername);
        Assert.Equal("culture-sport-admin", dto.TenantId);
        var stored = Assert.Single(_repo.Items);
        Assert.StartsWith("sha256:", stored.DefinitionHash);
        Assert.Contains(_audit.Records, r => r.Action == "create" && r.EntityType == "SavedQuery");
    }

    [Fact]
    public async Task Blank_name_is_a_validation_error()
    {
        await Assert.ThrowsAsync<ValidationException>(() => Service().Create(Request("   ")));
    }

    [Fact]
    public async Task Another_users_record_is_not_found_across_tenants()
    {
        var created = await Service().Create(Request());

        _user = new FakeCurrentUser("michal", "welfare-admin");

        await Assert.ThrowsAsync<NotFoundException>(() => Service().Get(created.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => Service().Delete(created.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => Service().Run(created.Id));
    }

    [Fact]
    public async Task Another_users_record_is_not_found_within_the_same_tenant()
    {
        var created = await Service().Create(Request());

        // Same tenant, different owner — scope is owner AND tenant.
        _user = new FakeCurrentUser("dan", "culture-sport-admin");

        await Assert.ThrowsAsync<NotFoundException>(() => Service().Get(created.Id));
        Assert.Empty(await Service().List());
    }

    [Fact]
    public async Task Run_executes_the_definition_updates_last_run_and_audits()
    {
        var created = await Service().Create(Request());

        var response = await Service().Run(created.Id);

        Assert.Same(_search.Response, response);
        var stored = Assert.Single(_repo.Items);
        Assert.NotNull(stored.LastRunAt);
        Assert.Equal(_search.Response.Page.TotalRows, stored.LastRunRowCount);
        Assert.Contains(_audit.Records, r => r.Action == "run" && r.EntityType == "SavedQuery");
    }

    [Fact]
    public async Task Delete_removes_the_record_and_audits()
    {
        _user = new FakeCurrentUser("dan", "culture-sport-admin", "admin");
        var created = await Service().Create(Request());

        await Service().Delete(created.Id);

        Assert.Empty(_repo.Items);
        Assert.Contains(_audit.Records, r => r.Action == "delete");
    }

    [Fact]
    public async Task Delete_by_a_non_admin_is_forbidden_and_keeps_the_record()
    {
        var created = await Service().Create(Request()); // default user: sarah, role 'analyst'

        await Assert.ThrowsAsync<ForbiddenException>(() => Service().Delete(created.Id));

        Assert.Single(_repo.Items);
        Assert.DoesNotContain(_audit.Records, r => r.Action == "delete");
    }

    private sealed class StubSearch : ISearchService
    {
        public SearchResponse Response { get; } = new(
            "q",
            [],
            [],
            new PageDto(1, 50, 7),
            new ExecutionMetaDto(1, 0, false, "sha256:x"));

        public Task<SearchResponse> Search(QueryDefinition definition, CancellationToken ct = default) =>
            Task.FromResult(Response);
    }
}
