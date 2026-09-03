using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Search.Interfaces;

/// <summary>
/// Runs a validated <see cref="QueryDefinition"/> against the data store: applies the tenant
/// scope, the whitelisted filters, and the segmentation, and returns the aggregated page.
/// Implemented in Infrastructure (EF Core).
/// </summary>
public interface ISearchQueryExecutor
{
    Task<QueryExecutionResult> Execute(
        QueryDefinition definition,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        CancellationToken ct = default);
}
