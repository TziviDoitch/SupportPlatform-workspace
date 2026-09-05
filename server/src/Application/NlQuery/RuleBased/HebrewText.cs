using System.Text;

namespace SupportPlatform.Application.NlQuery.RuleBased;

// The small amount of Hebrew morphology the rule-based parser needs to match free text against
// the metadata labels ("בתחום התרבות" -> the "תרבות" domain, "עמותות" -> "עמותה"). Deliberately
// crude: both sides of every comparison go through Normalize, so the stems only have to be
// consistent, not linguistically correct.
public static class HebrewText
{
    // Attached particles: ב/ה/ו/ל/מ/כ/ש. At most two ("במחוז" -> "מחוז" -> "חוז").
    private static readonly char[] Prefixes = ['ב', 'ה', 'ו', 'ל', 'מ', 'כ', 'ש'];

    // Trailing letters that vary with form and carry no meaning here ("שנה"/"שנת", "אושר"/"אושרו").
    private static readonly char[] WeakSuffixes = ['ה', 'ת', 'ו'];

    private static readonly string[] PluralSuffixes = ["ות", "ים"];

    private const int MaxPrefixStrips = 2;

    // Splits into word and number tokens; a letter/digit boundary also separates, so "ל2025" and
    // "2023-2025" yield usable year tokens.
    public static IReadOnlyList<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        bool? isDigitRun = null;

        foreach (var c in text)
        {
            if (!char.IsLetterOrDigit(c))
            {
                Flush();
                continue;
            }

            var isDigit = char.IsDigit(c);
            if (isDigitRun is not null && isDigitRun != isDigit)
                Flush();

            isDigitRun = isDigit;
            current.Append(c);
        }

        Flush();
        return tokens;

        void Flush()
        {
            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
                current.Clear();
            }
            isDigitRun = null;
        }
    }

    // Strip one plural or weak ending, then any attached particles. Endings go first so "שנה" and
    // "שנת" reduce alike. Latin tokens (reference codes) only get lower-cased.
    public static string Normalize(string word)
    {
        var w = StripEnding(word.ToLowerInvariant());

        for (var i = 0; i < MaxPrefixStrips && w.Length > 2 && Prefixes.Contains(w[0]); i++)
            w = w[1..];

        return w;
    }

    private static string StripEnding(string word)
    {
        foreach (var plural in PluralSuffixes)
            if (word.Length >= 4 && word.EndsWith(plural, StringComparison.Ordinal))
                return word[..^plural.Length];

        return word.Length >= 3 && WeakSuffixes.Contains(word[^1]) ? word[..^1] : word;
    }

    public static IReadOnlyList<string> Stems(string phrase) =>
        Tokenize(phrase).Select(Normalize).ToList();
}
