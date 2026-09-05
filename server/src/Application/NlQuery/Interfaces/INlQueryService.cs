namespace SupportPlatform.Application.NlQuery.Interfaces;

public interface INlQueryService
{
    Task<NlParseResponse> Parse(NlParseRequest request, CancellationToken ct = default);
}
