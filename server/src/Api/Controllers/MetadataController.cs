using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.Metadata;
using SupportPlatform.Application.Metadata.Interfaces;

namespace SupportPlatform.Api.Controllers;

[ApiController]
[Route("api/metadata")]
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
    public async Task<ActionResult<MetadataResponse>> Get([FromQuery] string? tenantId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
            return BadRequest("tenantId is required.");

        return await metadata.Get(tenantId, ct);
    }
}
