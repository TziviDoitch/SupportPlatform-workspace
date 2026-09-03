namespace SupportPlatform.Application.Auditing;

/// <summary>
/// Records "who did what, when". Called explicitly from use-case services on mutations and on
/// search — never wired as an EF interceptor (<c>IMPLEMENTATION_PLAN.md</c> §6 S5).
/// </summary>
public interface IAuditService
{
    /// <param name="payload">Optional object serialized to JSON as a snapshot of the request.</param>
    Task Record(string action, string entityType, string? entityId, object? payload, CancellationToken ct = default);
}
