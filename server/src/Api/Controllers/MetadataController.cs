using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.Metadata;
using SupportPlatform.Application.Metadata.Interfaces;

namespace SupportPlatform.Api.Controllers;

[ApiController]
[Route("api/metadata")]
[ProducesErrorResponseType(typeof(ProblemDetails))]
public class MetadataController(IMetadataService metadata) : ControllerBase
{
    /// <summary>
    /// Reference lists + filter-field registry for the dynamic search form.
    /// </summary>
    /// <remarks>
    /// S1 development contract: the tenant is passed as <c>?tenantId=</c>. When JWT auth lands
    /// in S8 the authenticated user's tenant becomes the authoritative scope; a client-supplied
    /// <c>tenantId</c> is not trusted for authorization.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType<MetadataResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MetadataResponse>> Get([FromQuery] string? tenantId, CancellationToken ct)
    {
        // Emit the same RFC 7807 shape as every other error path (docs/contracts/error-model.md);
        // never a hand-built string body.
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ValidationException([new ValidationFailure("tenantId", "'tenantId' is required.")]);

        return await metadata.Get(tenantId, ct);
    }
}
