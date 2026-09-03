using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;
using SupportPlatform.Application.Auditing;
using SupportPlatform.Application.Common;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.SavedQueries.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.SavedQueries;

/// <summary>
/// The S5 use case: CRUD + re-run for saved queries. Every record is scoped to the current
/// user + tenant; out-of-scope access is a <see cref="NotFoundException"/> (404, not 403, so
/// existence is not leaked). The definition is validated exactly like <c>POST /api/search</c>.
/// </summary>
public sealed class SavedQueryService(
    ISavedQueryRepository repo,
    ICurrentUser user,
    IValidator<QueryDefinition> validator,
    ISearchService search,
    IAuditService audit) : ISavedQueryService
{
    private static readonly JsonSerializerOptions Json = QueryDefinitionJson.Options;

    public async Task<IReadOnlyList<SavedQueryDto>> List(CancellationToken ct = default)
    {
        var rows = await repo.List(user.Username, user.TenantId, ct);
        return rows.Select(Map).ToList();
    }

    public async Task<SavedQueryDto> Get(Guid id, CancellationToken ct = default) => Map(await Require(id, ct));

    public async Task<SavedQueryDto> Create(SaveSavedQueryRequest request, CancellationToken ct = default)
    {
        var definition = await Validated(request, ct);

        var entity = new SavedQuery
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            DefinitionJson = JsonSerializer.Serialize(definition, Json),
            DefinitionHash = DefinitionHasher.Hash(definition),
            OwnerUsername = user.Username,
            TenantId = user.TenantId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await repo.Add(entity, ct);
        await repo.Save(ct);
        await audit.Record("create", "SavedQuery", entity.Id.ToString(), request, ct);
        return Map(entity);
    }

    public async Task<SavedQueryDto> Update(Guid id, SaveSavedQueryRequest request, CancellationToken ct = default)
    {
        var entity = await Require(id, ct);
        var definition = await Validated(request, ct);

        entity.Name = request.Name.Trim();
        entity.DefinitionJson = JsonSerializer.Serialize(definition, Json);
        entity.DefinitionHash = DefinitionHasher.Hash(definition);

        await repo.Save(ct);
        await audit.Record("update", "SavedQuery", id.ToString(), request, ct);
        return Map(entity);
    }

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        var entity = await Require(id, ct);
        await repo.Remove(entity, ct);
        await repo.Save(ct);
        await audit.Record("delete", "SavedQuery", id.ToString(), null, ct);
    }

    public async Task<SearchResponse> Run(Guid id, CancellationToken ct = default)
    {
        var entity = await Require(id, ct);
        var response = await search.Search(Deserialize(entity.DefinitionJson), ct);

        entity.LastRunAt = DateTimeOffset.UtcNow;
        entity.LastRunRowCount = response.Page.TotalRows;
        await repo.Save(ct);
        await audit.Record("run", "SavedQuery", id.ToString(), null, ct);
        return response;
    }

    private async Task<SavedQuery> Require(Guid id, CancellationToken ct) =>
        await repo.Find(id, user.Username, user.TenantId, ct)
        ?? throw new NotFoundException($"Saved query '{id}' was not found.");

    private async Task<QueryDefinition> Validated(SaveSavedQueryRequest request, CancellationToken ct)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length == 0)
            throw new ValidationException([new ValidationFailure("name", "Name is required.")]);
        if (name.Length > 200)
            throw new ValidationException([new ValidationFailure("name", "Name must be 200 characters or fewer.")]);

        await validator.ValidateAndThrowAsync(request.Definition, ct);

        // The record belongs to the caller's tenant; keep the stored definition consistent with it.
        // Enforcing a body/caller tenant match with a 403 is S8 (DESIGN_QA §3).
        return request.Definition with { TenantId = user.TenantId };
    }

    private static QueryDefinition Deserialize(string json) =>
        JsonSerializer.Deserialize<QueryDefinition>(json, Json)
        ?? throw new InvalidOperationException("Stored query definition could not be read.");

    private static SavedQueryDto Map(SavedQuery q) => new(
        q.Id, q.Name, Deserialize(q.DefinitionJson), q.OwnerUsername, q.TenantId,
        q.CreatedAt, q.LastRunAt, q.LastRunRowCount);
}
