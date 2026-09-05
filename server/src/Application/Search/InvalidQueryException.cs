namespace SupportPlatform.Application.Search;

// Defense-in-depth: a malformed QueryDefinition that got past FluentValidation to the query
// builder. Mapped to 400.
public sealed class InvalidQueryException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
