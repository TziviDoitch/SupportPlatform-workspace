using System.Text.Json;
using System.Text.Json.Serialization;

namespace SupportPlatform.Application.Search;

/// <summary>
/// Reads/writes the polymorphic <see cref="FilterValue"/>: a JSON array is a code list, a JSON
/// object with <c>"type": "range" | "single"</c> is a year filter. Shape from
/// <c>docs/contracts/query-definition.schema.json</c>.
/// </summary>
public sealed class FilterValueJsonConverter : JsonConverter<FilterValue>
{
    public override FilterValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.StartArray:
                var codes = JsonSerializer.Deserialize<List<string>>(ref reader, options) ?? [];
                return new FilterValue.Codes(codes);

            case JsonTokenType.StartObject:
                using (var doc = JsonDocument.ParseValue(ref reader))
                    return ReadYear(doc.RootElement);

            default:
                throw new JsonException($"A filter value must be an array or an object, got {reader.TokenType}.");
        }
    }

    private static FilterValue ReadYear(JsonElement e)
    {
        var type = e.TryGetProperty("type", out var t) ? t.GetString() : null;
        return type switch
        {
            "range" => new FilterValue.YearRange(GetInt(e, "from"), GetInt(e, "to")),
            "single" => new FilterValue.YearSingle(GetInt(e, "value")),
            _ => throw new JsonException("A year filter needs \"type\": \"range\" or \"single\".")
        };
    }

    private static int GetInt(JsonElement e, string name)
    {
        if (!e.TryGetProperty(name, out var v) || v.ValueKind != JsonValueKind.Number || !v.TryGetInt32(out var n))
            throw new JsonException($"A year filter needs an integer \"{name}\".");
        return n;
    }

    public override void Write(Utf8JsonWriter writer, FilterValue value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case FilterValue.Codes c:
                JsonSerializer.Serialize(writer, c.Values, options);
                break;
            case FilterValue.YearRange r:
                writer.WriteStartObject();
                writer.WriteString("type", "range");
                writer.WriteNumber("from", r.From);
                writer.WriteNumber("to", r.To);
                writer.WriteEndObject();
                break;
            case FilterValue.YearSingle s:
                writer.WriteStartObject();
                writer.WriteString("type", "single");
                writer.WriteNumber("value", s.Value);
                writer.WriteEndObject();
                break;
            default:
                throw new JsonException($"Unknown filter value {value.GetType().Name}.");
        }
    }
}
