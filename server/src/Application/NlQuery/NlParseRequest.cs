namespace SupportPlatform.Application.NlQuery;

// TenantId is optional; the caller's tenant is used when omitted.
public record NlParseRequest(string? Text, string? TenantId);
