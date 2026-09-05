using SupportPlatform.Application.Search;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.NlQuery.RuleBased.Rules;

// Reads the grouping clause ("לפי מחוז", "בפילוח לפי שנה") into Segmentation. A field is only
// considered after a grouping marker, so "בתחום התרבות" stays a filter while "לפי תחום" becomes
// a segmentation. A field matches on its full label, or on a label word unique to one segmentable
// field; an ambiguous word ("תמיכה", shared by two fields) matches neither.
internal static class SegmentationRule
{
    private static readonly IReadOnlyList<string>[] Markers = [HebrewText.Stems("לפי"), HebrewText.Stems("פילוח")];

    public static IReadOnlyList<string> Apply(NlText text, SearchMetadata meta)
    {
        var markers = Markers.Select(m => text.IndexOf(m)).Where(i => i >= 0).ToList();
        if (markers.Count == 0)
            return [];

        var marker = markers.Min();
        text.Claim(marker, 1);

        var fields = meta.Registry.Where(e => e.Segmentable).ToList();
        var unique = UniqueLabelWords(fields);

        var segmentation = new List<string>();
        foreach (var field in fields)
        {
            IReadOnlyList<string>? match = null;
            var matchAt = int.MaxValue;

            foreach (var candidate in Candidates(field, unique))
            {
                var at = text.IndexOf(candidate, marker + 1);
                if (at >= 0 && at < matchAt)
                {
                    matchAt = at;
                    match = candidate;
                }
            }

            if (match is null)
                continue;

            text.Claim(matchAt, match.Count);
            segmentation.Add(field.Id);
        }

        return segmentation;
    }

    // The full label first, then any label word that names this field unambiguously.
    private static IEnumerable<IReadOnlyList<string>> Candidates(
        FilterFieldRegistryEntry field, IReadOnlySet<string> unique)
    {
        var stems = HebrewText.Stems(field.Label);
        yield return stems;

        foreach (var word in stems.Where(unique.Contains))
            yield return [word];
    }

    // Label words that identify exactly one segmentable field.
    private static IReadOnlySet<string> UniqueLabelWords(IEnumerable<FilterFieldRegistryEntry> fields)
    {
        var counts = new Dictionary<string, int>();
        foreach (var field in fields)
            foreach (var word in HebrewText.Stems(field.Label).Distinct())
                counts[word] = counts.GetValueOrDefault(word) + 1;

        return counts.Where(p => p.Value == 1).Select(p => p.Key).ToHashSet();
    }
}
