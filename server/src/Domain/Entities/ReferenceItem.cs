namespace SupportPlatform.Domain.Entities;

/// <summary>
/// Base for a reference (lookup) row: a stable <see cref="Code"/> stored on data rows
/// plus a Hebrew display <see cref="Label"/>. Adding a row is a data change, not a code change (§8 Q1).
/// </summary>
public abstract class ReferenceItem
{
    public required string Code { get; set; }
    public required string Label { get; set; }
}
