namespace SupportPlatform.Api.Errors;

// The stable type / title pairs from docs/contracts/error-model.md.
public static class ProblemTypes
{
    private const string Base = "https://supportplatform.local/errors/";

    public static (string Type, string Title) ForStatus(int status) => status switch
    {
        400 => (Base + "validation", "One or more validation errors occurred."),
        // 401 has no live source in the PoC (X-User falls back to a seed user); kept for the
        // documented production contract.
        401 => (Base + "unauthorized", "Authentication required."),
        403 => (Base + "forbidden", "Access denied."),
        404 => (Base + "not-found", "Resource not found."),
        _ => (Base + "unexpected", "An unexpected error occurred.")
    };
}
