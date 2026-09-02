namespace SupportPlatform.Domain.Entities;

/// <summary>A government organization scope. Identified by a stable slug (e.g. "culture-sport-admin").</summary>
public class Tenant
{
    public required string Id { get; set; }
    public required string Name { get; set; }
}
