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
[ProducesErrorResponseType(typeof(ProblemDetails))]
public class SavedQueriesController(ISavedQueryService queries) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SavedQueryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SavedQueryDto>>> List(CancellationToken ct)
        => Ok(await queries.List(ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<SavedQueryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavedQueryDto>> Get(Guid id, CancellationToken ct)
        => Ok(await queries.Get(id, ct));

    [HttpPost]
    [ProducesResponseType<SavedQueryDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SavedQueryDto>> Create([FromBody] SaveSavedQueryRequest request, CancellationToken ct)
    {
        var created = await queries.Create(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<SavedQueryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SavedQueryDto>> Update(
        Guid id, [FromBody] SaveSavedQueryRequest request, CancellationToken ct)
        => Ok(await queries.Update(id, request, ct));

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await queries.Delete(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/run")]
    [ProducesResponseType<SearchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SearchResponse>> Run(Guid id, CancellationToken ct)
        => Ok(await queries.Run(id, ct));
}
