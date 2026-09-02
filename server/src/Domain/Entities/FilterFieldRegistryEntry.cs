namespace SupportPlatform.Domain.Entities;

/// <summary>
/// One row of the filter-field whitelist (<c>filter_field_registry</c>). It is the single
/// source of the canonical field ids used in <c>QueryDefinition</c> (§3.4 red line):
/// the client renders one form control per row, and <c>DynamicQueryBuilder</c> (S2) rejects
/// any field id not present here. A new filter field is a data change, not a code change (§8 Q1).
/// </summary>
public class FilterFieldRegistryEntry
{
    /// <summary>Canonical field id, e.g. "supportDomain". The only spelling used downstream.</summary>
    public required string Id { get; set; }

    public required string Label { get; set; }

    /// <summary>"codeList" or "yearRange" — drives the control type and accepted value shape.</summary>
    public required string Kind { get; set; }

    /// <summary>For "codeList" only — the reference list name that fills the control's options.</summary>
    public string? ReferenceList { get; set; }

    /// <summary>Allowed operators: a subset of "in", "range", "single".</summary>
    public required IReadOnlyList<string> Operators { get; set; }

    /// <summary>Whether this id may appear in <c>QueryDefinition.segmentation</c>.</summary>
    public bool Segmentable { get; set; }

    /// <summary>Position of this field's control in the form. The API emits entries in this order.</summary>
    public int SortOrder { get; set; }
}
