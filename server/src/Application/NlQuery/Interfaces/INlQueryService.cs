namespace SupportPlatform.Application.NlQuery.Interfaces;

/// <summary>Turns a free-text question into a reviewable, validated <c>QueryDefinition</c>.</summary>
public interface INlQueryService
{
    Task<NlParseResponse> Parse(NlParseRequest request, CancellationToken ct = default);
}
