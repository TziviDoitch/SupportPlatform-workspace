using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.SavedQueries;

/// <summary>A saved-query record on the wire (<c>docs/contracts/api-contract.md</c> §5).</summary>
public sealed record SavedQueryDto(
    Guid Id,
    string Name,
    QueryDefinition Definition,
    string OwnerUsername,
    string TenantId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastRunAt,
    int? LastRunRowCount);
