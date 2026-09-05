namespace SupportPlatform.Domain.Entities;

public abstract class ReferenceItem
{
    public required string Code { get; set; }
    public required string Label { get; set; }
}
