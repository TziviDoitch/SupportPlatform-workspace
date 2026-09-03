using System.Text.Json;

namespace SupportPlatform.Application.Search;

/// <summary>
/// Shared System.Text.Json options that understand the <see cref="FilterValue"/> hierarchy.
/// Use this anywhere a <see cref="QueryDefinition"/> is serialized off the wire (saved-query
/// storage, audit payloads) so the shape stays identical to the API contract.
/// </summary>
public static class QueryDefinitionJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new FilterValueJsonConverter() }
    };
}
