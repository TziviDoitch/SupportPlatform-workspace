namespace SupportPlatform.Domain.Entities;

/// <summary>
/// One recorded action — "who ran what, and when" (<c>IMPLEMENTATION_PLAN.md</c> §5,
/// <c>DESIGN_QA.md</c> §7). Written by explicit <c>IAuditService.Record</c> calls in the
/// use-case services, never by an EF interceptor. S5.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; }
    public required string User { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public required string CorrelationId { get; set; }

    /// <summary>Optional JSON snapshot of the request payload.</summary>
    public string? Payload { get; set; }
}
