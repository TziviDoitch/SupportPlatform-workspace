namespace SupportPlatform.Domain.Entities;

/// <summary>A single support request. Tenant-scoped; the core table the query engine (S2) runs over.</summary>
public class SupportRequest
{
    public Guid Id { get; set; }
    public required string TenantId { get; set; }

    public Guid SubmittingBodyId { get; set; }
    public SubmittingBody? SubmittingBody { get; set; }

    /// <summary>Reference code from <c>reference_domains</c>.</summary>
    public required string SupportDomainCode { get; set; }

    /// <summary>Reference code from <c>reference_statuses</c>.</summary>
    public required string StatusCode { get; set; }

    public int SupportYear { get; set; }

    public decimal AmountRequested { get; set; }
    public decimal AmountApproved { get; set; }
}
