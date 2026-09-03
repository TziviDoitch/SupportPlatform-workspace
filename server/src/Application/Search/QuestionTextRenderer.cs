using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Search;

/// <summary>
/// Renders the Hebrew read-back sentence for a <see cref="QueryDefinition"/> from the wording the
/// contract actually specifies (<c>docs/contracts/query-definition.md</c> "Reads as" +
/// <c>api-contract.md</c> §3): the opener "כמה בקשות תמיכה", the registry field labels, the
/// reference value labels, and the "בפילוח לפי" segmentation clause. No wording is invented
/// beyond that; the sum metric has no contract phrasing and is not narrated here.
/// </summary>
public sealed class QuestionTextRenderer
{
    private const string Opener = "כמה בקשות תמיכה";

    public string Render(QueryDefinition def, MetadataSnapshot meta)
    {
        var text = Opener;

        var clauses = FilterClauses(def, meta).ToList();
        if (clauses.Count > 0)
            text += " עם " + string.Join(", ", clauses);

        var segments = def.Segmentation
            .Select(id => meta.Registry.FirstOrDefault(e => e.Id == id)?.Label ?? id)
            .ToList();
        if (segments.Count > 0)
            text += ", בפילוח לפי " + string.Join(", ", segments);

        return text + "?";
    }

    private static IEnumerable<string> FilterClauses(QueryDefinition def, MetadataSnapshot meta)
    {
        foreach (var entry in meta.Registry.OrderBy(e => e.SortOrder))
        {
            if (!def.Filters.TryGetValue(entry.Id, out var value))
                continue;

            yield return $"{entry.Label}: {ValueText(value, entry, meta)}";
        }
    }

    private static string ValueText(FilterValue value, FilterFieldRegistryEntry entry, MetadataSnapshot meta) =>
        value switch
        {
            FilterValue.Codes c => string.Join(" או ", c.Values.Select(code => Label(code, entry, meta))),
            FilterValue.YearRange r => $"{r.From}–{r.To}",
            FilterValue.YearSingle s => s.Value.ToString(),
            _ => string.Empty
        };

    private static string Label(string code, FilterFieldRegistryEntry entry, MetadataSnapshot meta) =>
        meta.ReferenceList(entry.ReferenceList).FirstOrDefault(i => i.Code == code)?.Label ?? code;
}
