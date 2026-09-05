using System.Text.Json;

namespace SupportPlatform.Application.Search;

// Shared JSON options that understand the FilterValue hierarchy. Use wherever a QueryDefinition
// is serialized off the wire (saved-query storage, audit payloads).
public static class QueryDefinitionJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters = { new FilterValueJsonConverter() }
    };
}
