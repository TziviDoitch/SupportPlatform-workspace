using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Search.Filters.Interfaces;

namespace SupportPlatform.Infrastructure.Search;

/// <summary>
/// Translates a validated <see cref="QueryDefinition"/>'s filters into a safe
/// <see cref="IQueryable{SupportRequest}"/> (§3.4 red line). It only enforces the whitelist and
/// folds the resolved handlers — no per-field logic, no <c>switch</c>, no reflection.
/// </summary>
public sealed class DynamicQueryBuilder(IFilterHandlerResolver handlers)
{
    public IQueryable<SupportRequest> Apply(
        IQueryable<SupportRequest> source,
        QueryDefinition definition,
        IReadOnlyList<FilterFieldRegistryEntry> registry)
    {
        // Whitelist first: reject every unknown field id before any handler runs.
        foreach (var fieldId in definition.Filters.Keys)
            if (registry.All(e => e.Id != fieldId))
                throw new InvalidQueryException($"filters.{fieldId}", $"'{fieldId}' is not a known filter field.");

        foreach (var (fieldId, value) in definition.Filters)
        {
            var entry = registry.First(e => e.Id == fieldId);
            source = handlers.Resolve(entry).Apply(source, value);
        }

        return source;
    }
}
