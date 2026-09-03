using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.RuleBased.Rules;

/// <summary>
/// Matches reference values mentioned in the question against every <c>codeList</c> field in the
/// registry — one rule for all of them, because the vocabulary is metadata, not code. A domain,
/// body type, status or district added to the seed is recognised here with no code change
/// (<c>DESIGN_QA.md</c> Q1). Several values for one field become an IN list.
/// </summary>
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
