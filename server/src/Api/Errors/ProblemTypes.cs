namespace SupportPlatform.Api.Errors;

/// <summary>
/// The stable <c>type</c> / <c>title</c> pairs from <c>docs/contracts/error-model.md</c>.
/// </summary>
public static class ProblemTypes
{
    private const string Base = "https://supportplatform.local/errors/";

    public static (string Type, string Title) ForStatus(int status) => status switch
    {
        400 => (Base + "validation", "One or more validation errors occurred."),
        401 => (Base + "unauthorized", "Authentication required."),
        403 => (Base + "forbidden", "Access denied."),
        404 => (Base + "not-found", "Resource not found."),
        _ => (Base + "unexpected", "An unexpected error occurred.")
    };
}
