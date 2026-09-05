using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.SavedQueries;

public sealed record SavedQueryDto(
    Guid Id,
    string Name,
    QueryDefinition Definition,
    string OwnerUsername,
    string TenantId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastRunAt,
    int? LastRunRowCount);
