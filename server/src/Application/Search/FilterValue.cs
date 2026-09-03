namespace SupportPlatform.Application.Search;

/// <summary>
/// A single entry in <see cref="QueryDefinition.Filters"/>. Closed hierarchy — the three shapes
/// allowed by <c>docs/contracts/query-definition.md</c>: a code list (IN), a year range, or a
/// single year. The JSON form is handled by <see cref="FilterValueJsonConverter"/>.
/// </summary>
public abstract record FilterValue
{
    private FilterValue() { }

    /// <summary>One or more reference codes; IN semantics. Empty is invalid (omit the key instead).</summary>
    public sealed record Codes(IReadOnlyList<string> Values) : FilterValue;

    /// <summary>Inclusive year range. <see cref="From"/> must be &lt;= <see cref="To"/>.</summary>
    public sealed record YearRange(int From, int To) : FilterValue;

    /// <summary>Exact year.</summary>
    public sealed record YearSingle(int Value) : FilterValue;
}
