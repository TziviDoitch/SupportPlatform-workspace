namespace SupportPlatform.Domain.Entities;

// Stored as canonical QueryDefinition JSON + its hash; scoped explicitly by owner + tenant.
public class SavedQuery
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DefinitionJson { get; set; }
    public required string DefinitionHash { get; set; }
    public required string OwnerUsername { get; set; }
    public required string TenantId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public int? LastRunRowCount { get; set; }
}
