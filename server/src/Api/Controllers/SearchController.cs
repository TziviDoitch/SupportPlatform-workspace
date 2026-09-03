using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Api.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController(ISearchService search) : ControllerBase
{
    /// <summary>Run a <see cref="QueryDefinition"/>: rows + aggregations + question text.</summary>
    [HttpPost]
    public async Task<ActionResult<SearchResponse>> Post([FromBody] QueryDefinition definition, CancellationToken ct)
        => Ok(await search.Search(definition, ct));
}
