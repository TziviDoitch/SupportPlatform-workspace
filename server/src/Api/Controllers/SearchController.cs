using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Api.Controllers;

[ApiController]
[Route("api/search")]
[ProducesErrorResponseType(typeof(ProblemDetails))]
public class SearchController(ISearchService search) : ControllerBase
{
    /// <summary>Run a <see cref="QueryDefinition"/>: rows + aggregations + question text.</summary>
    [HttpPost]
    [ProducesResponseType<SearchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SearchResponse>> Post([FromBody] QueryDefinition definition, CancellationToken ct)
        => Ok(await search.Search(definition, ct));
}
