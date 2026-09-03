using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

/// <summary>
/// The registered filter handlers — one per <c>filter_field_registry</c> row, each pairing a
/// field id with its strongly-typed column selector. A new filter field is one more line here;
/// a new <c>kind</c> is one more <see cref="FilterHandler"/> subclass.
/// </summary>
public static class FilterHandlers
{
    public static IReadOnlyList<FilterHandler> Default { get; } =
    [
        new CodeListFilterHandler("bodyType", r => r.SubmittingBody!.BodyTypeCode),
        new CodeListFilterHandler("supportDomain", r => r.SupportDomainCode),
        new CodeListFilterHandler("status", r => r.StatusCode),
        new CodeListFilterHandler("district", r => r.SubmittingBody!.DistrictCode),
        new YearRangeFilterHandler("supportYear", r => r.SupportYear)
    ];
}
