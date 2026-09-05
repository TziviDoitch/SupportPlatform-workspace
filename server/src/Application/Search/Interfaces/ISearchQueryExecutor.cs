using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Search.Interfaces;

// Runs a validated QueryDefinition against the data store and returns every aggregated group;
// ordering + paging are applied afterwards by BucketPaging. Implemented in Infrastructure.
public interface ISearchQueryExecutor
{
    Task<IReadOnlyList<AggregateBucket>> Execute(
        QueryDefinition definition,
        IReadOnlyList<FilterFieldRegistryEntry> registry,
        CancellationToken ct = default);
}
