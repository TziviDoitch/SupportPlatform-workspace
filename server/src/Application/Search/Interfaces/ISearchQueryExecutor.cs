using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Search.Interfaces;

/// <summary>
/// Runs a validated <see cref="QueryDefinition"/> against the data store: applies the tenant
/// scope, the whitelisted filters, and the segmentation, and returns every aggregated group
/// (ordering + paging are applied afterwards by <see cref="BucketPaging"/>). Implemented in
/// Infrastructure (EF Core).
/// </summary>
public interface ISearchQueryExecutor
{
    Task<IReadOnlyList<AggregateBucket>> Execute(
        QueryDefinition definition,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        CancellationToken ct = default);
}
