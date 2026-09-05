using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters.Interfaces;

public interface IFilterHandlerResolver
{
    FilterHandler Resolve(FilterFieldRegistryEntry field);
}
