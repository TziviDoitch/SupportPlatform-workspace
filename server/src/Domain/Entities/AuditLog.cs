namespace SupportPlatform.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; set; }
    public required string User { get; set; }
    public required string Action { get; set; }
    public required string EntityType { get; set; }
    public string? EntityId { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public required string CorrelationId { get; set; }
    public string? Payload { get; set; }
}
