using FluentValidation;
using FluentValidation.Results;
using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.NlQuery;

// Owns everything around the AI seam so the provider stays a pure translation: fetch the
// vocabulary, hand it to the provider, then run the definition it produced through the same
// validator and Hebrew renderer that POST /api/search uses. Parsing does not execute a search.
public sealed class NlQueryService(
    INlQueryProvider provider,
    ISearchMetadataProvider metadata,
    IValidator<QueryDefinition> validator,
    QuestionTextRenderer questionText,
    TenantAccessGuard tenantAccess,
    IAuditService audit) : INlQueryService
{
    public async Task<NlParseResponse> Parse(NlParseRequest request, CancellationToken ct = default)
    {
        var text = request.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
            throw new ValidationException([new ValidationFailure("text", "text is required.")]);

        // Identity is authoritative for the tenant: use the caller's when omitted, 403 on a mismatch.
        var tenantId = tenantAccess.EnsureTenant(request.TenantId);

        var meta = await metadata.Get(ct);
        var result = await provider.Translate(text, tenantId, meta, ct);

        // A provider is not trusted to produce a runnable query — the whitelist decides.
        await validator.ValidateAndThrowAsync(result.Definition, ct);

        await audit.Record("nl-parse", "QueryDefinition", null, request, ct);

        return new NlParseResponse(
            result.Definition,
            questionText.Render(result.Definition, meta.Snapshot),
            result.Confidence,
            result.Unresolved);
    }
}
