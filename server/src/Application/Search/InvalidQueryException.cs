namespace SupportPlatform.Application.Search;

/// <summary>
/// A <see cref="QueryDefinition"/> that reached the query builder still malformed — an unknown
/// filter field, or a value whose shape doesn't match the registry entry. FluentValidation
/// normally catches these first; the builder throws this as defense-in-depth. Mapped to 400.
/// </summary>
public sealed class InvalidQueryException(string field, string message) : Exception(message)
{
    /// <summary>Request-body path of the offending value, e.g. <c>filters.costCenter</c>.</summary>
    public string Field { get; } = field;
}
