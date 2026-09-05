namespace SupportPlatform.Domain.Entities;

public class SupportRequest
{
    public Guid Id { get; set; }
    public required string TenantId { get; set; }

    public Guid SubmittingBodyId { get; set; }
    public SubmittingBody? SubmittingBody { get; set; }

    public required string SupportDomainCode { get; set; }
    public required string StatusCode { get; set; }

    public int SupportYear { get; set; }

    public decimal AmountRequested { get; set; }
    public decimal AmountApproved { get; set; }
}
