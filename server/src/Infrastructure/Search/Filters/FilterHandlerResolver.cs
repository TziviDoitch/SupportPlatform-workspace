using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Search.Filters.Interfaces;

namespace SupportPlatform.Infrastructure.Search.Filters;

// Indexes the registered handlers by field id — registration replaces a switch. A new filter
// field is one more registration; a new kind is one more subclass. This class never changes.
public sealed class FilterHandlerResolver : IFilterHandlerResolver
{
    private readonly IReadOnlyDictionary<string, FilterHandler> _byFieldId;

    public FilterHandlerResolver(IEnumerable<FilterHandler> handlers)
    {
        _byFieldId = handlers.ToDictionary(h => h.FieldId);
    }

    public FilterHandler Resolve(FilterFieldRegistryEntry field)
    {
        if (!_byFieldId.TryGetValue(field.Id, out var handler))
            throw new InvalidOperationException($"No filter handler is registered for field '{field.Id}'.");

        if (handler.Kind != field.Kind)
            throw new InvalidOperationException(
                $"Field '{field.Id}' is kind '{field.Kind}' but its handler implements '{handler.Kind}'.");

        return handler;
    }
}
