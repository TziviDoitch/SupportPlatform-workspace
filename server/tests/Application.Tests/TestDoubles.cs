using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.SavedQueries.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Tests;

internal sealed class FakeCurrentUser(
    string username = "sarah",
    string tenantId = "culture-sport-admin",
    string role = "analyst") : ICurrentUser
{
    public string Username { get; } = username;
    public string TenantId { get; } = tenantId;
    public string Role { get; } = role;
    public string CorrelationId { get; } = "test-corr";
}

internal sealed class RecordingAuditService : IAuditService
{
    public List<(string Action, string EntityType, string? EntityId)> Records { get; } = [];

    public Task Record(string action, string entityType, string? entityId, object? payload, CancellationToken ct = default)
    {
        Records.Add((action, entityType, entityId));
        return Task.CompletedTask;
    }
}

internal sealed class FakeSavedQueryRepository : ISavedQueryRepository
{
    public List<SavedQuery> Items { get; } = [];

    public Task<IReadOnlyList<SavedQuery>> List(string ownerUsername, string tenantId, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<SavedQuery>>(
            Items.Where(q => q.OwnerUsername == ownerUsername && q.TenantId == tenantId).ToList());

    public Task<SavedQuery?> Find(Guid id, string ownerUsername, string tenantId, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(q =>
            q.Id == id && q.OwnerUsername == ownerUsername && q.TenantId == tenantId));

    public Task Add(SavedQuery query, CancellationToken ct = default)
    {
        Items.Add(query);
        return Task.CompletedTask;
    }

    public Task Save(CancellationToken ct = default) => Task.CompletedTask;

    public Task Remove(SavedQuery query, CancellationToken ct = default)
    {
        Items.Remove(query);
        return Task.CompletedTask;
    }
}
