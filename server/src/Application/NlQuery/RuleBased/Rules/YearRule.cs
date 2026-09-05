using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.RuleBased.Rules;

// Reads the years from the question into the registry's yearRange field: one year -> single
// value, two or more -> the inclusive range they span. A 4-digit number outside a plausible
// calendar range is left unclaimed. Skipped unless the registry has exactly one year field —
// which one was meant would be a guess.
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
