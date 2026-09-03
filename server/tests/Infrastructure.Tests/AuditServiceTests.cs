using SupportPlatform.Application.Identity;
using SupportPlatform.Infrastructure.Auditing;

namespace SupportPlatform.Infrastructure.Tests;

public class AuditServiceTests
{
    private sealed class StubUser : ICurrentUser
    {
        public string Username => "sarah";
        public string TenantId => "culture-sport-admin";
        public string Role => "analyst";
        public string CorrelationId => "corr-42";
    }

    [Fact]
    public async Task Record_writes_a_row_with_user_correlation_and_payload()
    {
        using var testDb = new TestDb();
        var service = new AuditService(testDb.Context, new StubUser());

        await service.Record("create", "SavedQuery", "id-1", new { name = "x" });

        var row = Assert.Single(testDb.NewContext().AuditLogs);
        Assert.Equal("sarah", row.User);
        Assert.Equal("create", row.Action);
        Assert.Equal("SavedQuery", row.EntityType);
        Assert.Equal("id-1", row.EntityId);
        Assert.Equal("corr-42", row.CorrelationId);
        Assert.Contains("\"name\"", row.Payload);
        Assert.NotEqual(default, row.OccurredAt);
    }

    [Fact]
    public async Task Record_allows_a_null_payload()
    {
        using var testDb = new TestDb();
        var service = new AuditService(testDb.Context, new StubUser());

        await service.Record("delete", "SavedQuery", "id-2", null);

        var row = Assert.Single(testDb.NewContext().AuditLogs);
        Assert.Null(row.Payload);
    }
}
