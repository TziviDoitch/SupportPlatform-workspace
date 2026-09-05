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
            var codes = meta.Snapshot.ReferenceList(entry.ReferenceList)
                .Where(item => text.TryClaim(HebrewText.Stems(item.Label)) ||
                               text.TryClaim(HebrewText.Stems(item.Code)))
                .Select(item => item.Code)
                .ToList();

            if (codes.Count > 0)
                filters[entry.Id] = new FilterValue.Codes(codes);
        }
    }
}
