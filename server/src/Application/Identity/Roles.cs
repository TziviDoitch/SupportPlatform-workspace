namespace SupportPlatform.Application.Identity;

/// <summary>
/// The seed role names (see <c>Infrastructure/Persistence/DbSeeder.cs</c>). The PoC enforces
/// exactly one role rule — deleting a saved query requires <see cref="Admin"/>
/// (<c>SavedQueryService.Delete</c>, <c>docs/DESIGN_QA.md</c> §3).
/// </summary>
public static class Roles
{
    public const string Analyst = "analyst";
    public const string Admin = "admin";
}
