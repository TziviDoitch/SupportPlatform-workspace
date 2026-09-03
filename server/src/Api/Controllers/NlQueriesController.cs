using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.NlQuery;
using SupportPlatform.Application.NlQuery.Interfaces;

namespace SupportPlatform.Api.Controllers;

[ApiController]
[Route("api/nl-queries")]
[ProducesErrorResponseType(typeof(ProblemDetails))]
public class NlQueriesController(INlQueryService nlQuery) : ControllerBase
{
    /// <summary>
    /// Free text → a reviewable <c>QueryDefinition</c> + Hebrew interpretation. Does not run the
    /// query; the client posts the definition to <c>/api/search</c> after the user confirms.
    /// </summary>
    [HttpPost("parse")]
    [ProducesResponseType<NlParseResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<NlParseResponse>> Parse([FromBody] NlParseRequest request, CancellationToken ct)
        => Ok(await nlQuery.Parse(request, ct));
}
