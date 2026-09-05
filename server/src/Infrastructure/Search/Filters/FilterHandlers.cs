using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Search.Filters;

// One handler per filter_field_registry row, pairing a field id with its typed column selector.
// A new filter field is one more line here; a new kind is one more FilterHandler subclass.
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
