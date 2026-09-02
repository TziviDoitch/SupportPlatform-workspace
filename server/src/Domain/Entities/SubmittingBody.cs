namespace SupportPlatform.Domain.Entities;

/// <summary>An organization that submits support requests. Tenant-scoped.</summary>
public class SubmittingBody
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TenantId { get; set; }

    /// <summary>Reference code from <c>reference_body_types</c>.</summary>
    public required string BodyTypeCode { get; set; }

    /// <summary>Reference code from <c>reference_districts</c>.</summary>
    public required string DistrictCode { get; set; }
}
