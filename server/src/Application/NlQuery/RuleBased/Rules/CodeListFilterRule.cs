using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.RuleBased.Rules;

// Matches reference values in the question against every codeList field in the registry — one
// rule for all of them, because the vocabulary is metadata. Several values for one field become
// an IN list.
internal static class CodeListFilterRule
{
    public static void Apply(NlText text, SearchMetadata meta, IDictionary<string, FilterValue> filters)
    {
        foreach (var entry in meta.Registry.Where(e => e.Kind == FieldKind.CodeList))
        {
            var items = meta.Snapshot.ReferenceList(entry.ReferenceList);

            // Longest label first: "אירועי תרבות" has to claim both its words before plain
            // "תרבות" can take one of them. The result keeps the reference list's own order.
            var matched = items
                .OrderByDescending(item => HebrewText.Stems(item.Label).Count)
                .Where(item => text.TryClaim(HebrewText.Stems(item.Label)) ||
                               text.TryClaim(HebrewText.Stems(item.Code)))
                .Select(item => item.Code)
                .ToHashSet();

            if (matched.Count > 0)
                filters[entry.Id] = new FilterValue.Codes(
                    [.. items.Select(i => i.Code).Where(matched.Contains)]);
        }
    }
}
