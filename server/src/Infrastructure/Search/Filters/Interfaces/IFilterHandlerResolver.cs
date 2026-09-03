using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters.Interfaces;

/// <summary>Finds the <see cref="FilterHandler"/> registered for a whitelisted registry field.</summary>
public interface IFilterHandlerResolver
{
    FilterHandler Resolve(FilterFieldRegistryEntry field);
}
