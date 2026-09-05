namespace SupportPlatform.Domain.Entities;

public class FilterFieldRegistryEntry
{
    public required string Id { get; set; }
    public required string Label { get; set; }
    public required string Kind { get; set; }
    public string? ReferenceList { get; set; }
    public required IReadOnlyList<string> Operators { get; set; }
    public bool Segmentable { get; set; }
    public int SortOrder { get; set; }
}
