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
    // Reference lists + filter-field registry for the dynamic search form. The ?tenantId= is
    // validated against identity, not trusted for authorization.
    [HttpGet]
    [ProducesResponseType<MetadataResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MetadataResponse>> Get([FromQuery] string? tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            throw new ValidationException([new ValidationFailure("tenantId", "'tenantId' is required.")]);

        return await metadata.Get(tenantId, ct);
    }
}
