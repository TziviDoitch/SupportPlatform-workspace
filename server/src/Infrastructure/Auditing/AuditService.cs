using System.Text.Json;
using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Infrastructure.Auditing;

/// <summary>
/// Writes one <see cref="AuditLog"/> row per call, stamped with the current user and the
/// request correlation id. Invoked explicitly by the use-case services (S5).
/// </summary>
public sealed class AuditService(SupportPlatformDbContext db, ICurrentUser user) : IAuditService
{
    public async Task Record(
        string action, string entityType, string? entityId, object? payload, CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            Id = Guid.NewGuid(),
            User = user.Username,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OccurredAt = DateTimeOffset.UtcNow,
            CorrelationId = user.CorrelationId,
            Payload = payload is null ? null : JsonSerializer.Serialize(payload, QueryDefinitionJson.Options)
        });

        await db.SaveChangesAsync(ct);
    }
}
