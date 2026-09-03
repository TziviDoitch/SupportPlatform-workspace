using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.RuleBased.Rules;

/// <summary>
/// Reads the years out of the question and fills the registry's <c>yearRange</c> field: one year
/// becomes a single value, two or more become the inclusive range they span ("בין 2023 ל-2025").
/// A 4-digit number outside a plausible calendar range is not a year — it stays unclaimed and is
/// reported as unresolved rather than turned into a filter nobody asked for.
/// Skipped when the registry has anything other than exactly one year field — which one was meant
/// would be a guess, and the words are left in <c>unresolved</c> instead.
/// </summary>
internal static class YearRule
{
    private const int MinYear = 1900;
    private const int MaxYear = 2100;

    public static void Apply(NlText text, SearchMetadata meta, IDictionary<string, FilterValue> filters)
    {
        var fields = meta.Registry.Where(e => e.Kind == FieldKind.YearRange).ToList();
        if (fields.Count != 1)
            return;

        var years = text.Years().Where(y => y.Value is >= MinYear and <= MaxYear).ToList();
        if (years.Count == 0)
            return;

        foreach (var (index, _) in years)
            text.Claim(index, 1);

        var values = years.Select(y => y.Value).ToList();
        filters[fields[0].Id] = values.Count == 1
            ? new FilterValue.YearSingle(values[0])
            : new FilterValue.YearRange(values.Min(), values.Max());
    }
}
