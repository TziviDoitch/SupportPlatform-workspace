namespace SupportPlatform.Application.NlQuery.RuleBased;

/// <summary>
/// The question under analysis: its tokens, their stems, and which of them a rule has already
/// claimed. Claiming is what makes <c>unresolved</c> and <c>confidence</c> meaningful — whatever
/// no rule claimed is reported back to the user rather than guessed at.
/// </summary>
internal sealed class NlText
{
    private readonly string[] _tokens;
    private readonly string[] _stems;
    private readonly bool[] _claimed;
    private readonly bool[] _meaningful;

    public NlText(string text)
    {
        _tokens = HebrewText.Tokenize(text).ToArray();
        _stems = _tokens.Select(HebrewText.Normalize).ToArray();
        _claimed = new bool[_tokens.Length];
        // A lone particle ("ל" of "ל-2025") is noise, not something the parser failed to understand.
        _meaningful = _tokens.Select(t => t.Length > 1 && !HebrewStopWords.Is(t)).ToArray();
    }

    /// <summary>Index of the first unclaimed occurrence of <paramref name="stems"/> at or after
    /// <paramref name="from"/>, or -1.</summary>
    public int IndexOf(IReadOnlyList<string> stems, int from = 0)
    {
        if (stems.Count == 0)
            return -1;

        for (var start = Math.Max(from, 0); start + stems.Count <= _stems.Length; start++)
        {
            var matches = true;
            for (var offset = 0; offset < stems.Count && matches; offset++)
                matches = !_claimed[start + offset] && _stems[start + offset] == stems[offset];

            if (matches)
                return start;
        }

        return -1;
    }

    /// <summary>Claims <paramref name="stems"/> where it first occurs. True when it was present.</summary>
    public bool TryClaim(IReadOnlyList<string> stems)
    {
        var start = IndexOf(stems);
        if (start < 0)
            return false;

        Claim(start, stems.Count);
        return true;
    }

    public void Claim(int start, int length)
    {
        for (var i = start; i < start + length; i++)
            _claimed[i] = true;
    }

    /// <summary>Unclaimed 4-digit tokens, with their positions, in reading order.</summary>
    public IEnumerable<(int Index, int Value)> Years()
    {
        for (var i = 0; i < _tokens.Length; i++)
            if (!_claimed[i] && _tokens[i].Length == 4 && int.TryParse(_tokens[i], out var year))
                yield return (i, year);
    }

    /// <summary>Meaningful words no rule claimed — reported to the user as <c>unresolved</c>.</summary>
    public IReadOnlyList<string> Unclaimed() =>
        Enumerable.Range(0, _tokens.Length)
            .Where(i => !_claimed[i] && _meaningful[i])
            .Select(i => _tokens[i])
            .Distinct()
            .ToList();

    /// <summary>Share of the meaningful words a rule claimed; 0 when nothing was understood.</summary>
    public double Coverage()
    {
        var meaningful = Enumerable.Range(0, _tokens.Length).Where(i => _meaningful[i]).ToList();
        if (meaningful.Count == 0)
            return 0;

        return Math.Round((double)meaningful.Count(i => _claimed[i]) / meaningful.Count, 2);
    }
}
