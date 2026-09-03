using FluentValidation;
using FluentValidation.Results;
using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.NlQuery;

/// <summary>
/// The S6 use case. It owns everything around the AI seam so the provider itself stays a pure
/// translation: fetch the vocabulary, hand it to <see cref="INlQueryProvider"/>, then run the
/// definition it produced through the same <c>QueryDefinition</c> validator and Hebrew renderer
/// that <c>POST /api/search</c> uses.
///
/// Parsing does not execute a search — the client reviews the interpretation first and then posts
/// the definition to <c>/api/search</c> (<c>api-contract.md</c> §4).
/// </summary>
public sealed class NlQueryService(
    INlQueryProvider provider,
    ISearchMetadataProvider metadata,
    IValidator<QueryDefinition> validator,
    QuestionTextRenderer questionText,
    ICurrentUser user,
    IAuditService audit) : INlQueryService
{
    public async Task<NlParseResponse> Parse(NlParseRequest request, CancellationToken ct = default)
    {
        var text = request.Text?.Trim() ?? string.Empty;
        if (text.Length == 0)
            throw new ValidationException([new ValidationFailure("text", "text is required.")]);

        var tenantId = string.IsNullOrWhiteSpace(request.TenantId) ? user.TenantId : request.TenantId;

        var meta = await metadata.Get(ct);
        var result = await provider.Parse(text, tenantId, meta, ct);

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
