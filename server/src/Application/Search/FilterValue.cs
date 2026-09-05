namespace SupportPlatform.Application.Search;

// Closed hierarchy of the three filter shapes: code list (IN), year range, single year.
// JSON form handled by FilterValueJsonConverter.
public abstract record FilterValue
{
    private FilterValue() { }

    public sealed record Codes(IReadOnlyList<string> Values) : FilterValue;

    public sealed record YearRange(int From, int To) : FilterValue;

    public sealed record YearSingle(int Value) : FilterValue;
}
