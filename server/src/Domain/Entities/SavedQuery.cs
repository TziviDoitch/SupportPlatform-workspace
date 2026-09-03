namespace SupportPlatform.Domain.Entities;

/// <summary>
/// A <c>QueryDefinition</c> a user chose to keep (<c>docs/contracts/api-contract.md</c> §5).
/// The definition is stored as canonical JSON plus its <see cref="DefinitionHash"/>; scoping is
/// explicit (owner + tenant), not a global query filter. S5.
/// </summary>
public class SavedQuery
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    /// <summary>The canonical <c>QueryDefinition</c> JSON. Re-validated on every write.</summary>
    public required string DefinitionJson { get; set; }

    /// <summary>Canonical SHA-256 of the definition (see <c>DefinitionHasher</c>).</summary>
    public required string DefinitionHash { get; set; }

    public required string OwnerUsername { get; set; }
    public required string TenantId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public int? LastRunRowCount { get; set; }
}
