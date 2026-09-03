namespace SupportPlatform.Application.Search;

/// <summary>The <c>kind</c> values a <c>filter_field_registry</c> row may carry
/// (<c>docs/contracts/metadata-model.md</c>). Drives which filter handler applies.</summary>
public static class FieldKind
{
    public const string CodeList = "codeList";
    public const string YearRange = "yearRange";
}
