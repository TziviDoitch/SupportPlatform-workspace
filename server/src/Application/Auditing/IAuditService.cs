namespace SupportPlatform.Application.Auditing;

// Records "who did what, when". Called explicitly from use-case services — never an EF interceptor.
// payload is an optional object serialized to JSON as a snapshot of the request.
public interface IAuditService
{
    Task Record(string action, string entityType, string? entityId, object? payload, CancellationToken ct = default);
}
