namespace SupportPlatform.Application.NlQuery.RuleBased;

/// <summary>
/// Function words and question scaffolding that carry no filter meaning. Only used to keep
/// <c>unresolved</c> honest — a word listed here is not reported back as "not understood".
/// Grammar, not business vocabulary: filter values always come from the metadata.
/// </summary>
internal static class HebrewStopWords
{
    private static readonly HashSet<string> Stems =
        new(new[]
            {
                "כמה", "מה", "מי", "איזה", "הצג", "תציג", "הראה", "תראה", "רוצה", "אני", "לי",
                "את", "של", "עם", "כל", "יש", "בבקשה", "אנא", "ו", "או", "גם", "בין", "עד",
                "לפי", "פילוח", "בקשה", "תמיכה", "נתונים", "רשימה", "סך", "מספר", "היו", "הן", "הם"
            }
            .Select(HebrewText.Normalize));

    public static bool Is(string token) => Stems.Contains(HebrewText.Normalize(token));
}
