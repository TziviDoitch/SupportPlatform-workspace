namespace SupportPlatform.Application.NlQuery;

/// <summary>Request body of <c>POST /api/nl-queries/parse</c> (<c>api-contract.md</c> §4).</summary>
/// <param name="Text">The free-text question. Required.</param>
/// <param name="TenantId">Optional; the caller's tenant is used when omitted.</param>
public record NlParseRequest(string? Text, string? TenantId);
