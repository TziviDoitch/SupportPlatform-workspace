namespace SupportPlatform.Domain.Entities;

public class SubmittingBody
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string TenantId { get; set; }
    public required string BodyTypeCode { get; set; }
    public required string DistrictCode { get; set; }
}
