namespace SupportPlatform.Application.Identity;

// The PoC enforces one role rule: deleting a saved query requires Admin.
public static class Roles
{
    public const string Analyst = "analyst";
    public const string Admin = "admin";
}
