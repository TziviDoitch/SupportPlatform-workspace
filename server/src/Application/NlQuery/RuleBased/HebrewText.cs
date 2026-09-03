using System.Text;

namespace SupportPlatform.Application.NlQuery.RuleBased;

/// <summary>
/// The small amount of Hebrew morphology the rule-based parser needs in order to match free text
/// against the metadata labels ("בתחום התרבות" → the "תרבות" domain, "עמותות" → "עמותה").
///
/// It is deliberately crude: both sides of every comparison go through <see cref="Normalize"/>,
/// so the stems only have to be *consistent*, not linguistically correct. No business value is
/// encoded here — the vocabulary always comes from the metadata.
/// </summary>
public static class HebrewText
{
    /// <summary>Attached particles: ב/ה/ו/ל/מ/כ/ש. At most two ("במחוז" → "מחוז" → "חוז").</summary>
    private static readonly char[] Prefixes = ['ב', 'ה', 'ו', 'ל', 'מ', 'כ', 'ש'];

    /// <summary>Trailing letters that vary with form and carry no meaning here ("שנה"/"שנת", "אושר"/"אושרו").</summary>
    private static readonly char[] WeakSuffixes = ['ה', 'ת', 'ו'];

    private static readonly string[] PluralSuffixes = ["ות", "ים"];

    private const int MaxPrefixStrips = 2;

    /// <summary>
    /// Splits into word and number tokens; punctuation separates, and a letter/digit boundary
    /// also separates so "ל2025" and "2023-2025" yield usable year tokens.
    /// </summary>
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

    /// <summary>
    /// Reduces one token to the stem used for matching: strip one plural or weak ending, then any
    /// attached particles. Endings go first so "שנה" and "שנת" reduce alike — taking the ש of
    /// "שנה" for an attached particle would pull them apart. Latin tokens (reference codes) only
    /// get lower-cased.
    /// </summary>
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

    /// <summary>The normalized stems of a label or phrase, in order.</summary>
    public static IReadOnlyList<string> Stems(string phrase) =>
        Tokenize(phrase).Select(Normalize).ToList();
}
