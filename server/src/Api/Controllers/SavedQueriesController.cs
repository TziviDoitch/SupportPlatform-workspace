using Microsoft.AspNetCore.Mvc;
using SupportPlatform.Application.SavedQueries;
using SupportPlatform.Application.SavedQueries.Interfaces;
using SupportPlatform.Application.Search;

namespace SupportPlatform.Api.Controllers;

/// <summary>
/// CRUD + re-run for saved queries (<c>docs/contracts/api-contract.md</c> §5–6). Scope (owner +
/// tenant) and validation live in <see cref="ISavedQueryService"/>; this only forwards.
/// </summary>
[ApiController]
[Route("api/saved-queries")]
public class SavedQueriesController(ISavedQueryService queries) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SavedQueryDto>>> List(CancellationToken ct)
        => Ok(await queries.List(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SavedQueryDto>> Get(Guid id, CancellationToken ct)
        => Ok(await queries.Get(id, ct));

    [HttpPost]
    public async Task<ActionResult<SavedQueryDto>> Create([FromBody] SaveSavedQueryRequest request, CancellationToken ct)
    {
        var created = await queries.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SavedQueryDto>> Update(
        Guid id, [FromBody] SaveSavedQueryRequest request, CancellationToken ct)
        => Ok(await queries.Update(id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await queries.Delete(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/run")]
    public async Task<ActionResult<SearchResponse>> Run(Guid id, CancellationToken ct)
        => Ok(await queries.Run(id, ct));
}
