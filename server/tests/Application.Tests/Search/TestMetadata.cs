using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Application.Tests.Search;

/// <summary>The frozen S1 reference lists + 5-row registry, for validator / renderer tests.</summary>
internal static class TestMetadata
{
    public static MetadataSnapshot Snapshot { get; } = new(
        Domains:
        [
            new ReferenceDomain { Code = "culture", Label = "תרבות" },
            new ReferenceDomain { Code = "sport", Label = "ספורט" }
        ],
        BodyTypes:
        [
            new ReferenceBodyType { Code = "association", Label = "עמותה" },
            new ReferenceBodyType { Code = "company", Label = "חברה" }
        ],
        Statuses:
        [
            new ReferenceStatus { Code = "approved", Label = "מאושר" },
            new ReferenceStatus { Code = "pending", Label = "בבדיקה" },
            new ReferenceStatus { Code = "rejected", Label = "נדחה" }
        ],
        Districts:
        [
            new ReferenceDistrict { Code = "north", Label = "צפון" },
            new ReferenceDistrict { Code = "center", Label = "מרכז" },
            new ReferenceDistrict { Code = "south", Label = "דרום" }
        ],
        Registry:
        [
            Field("bodyType", "סוג גוף", FieldKind.CodeList, "bodyTypes", ["in"], true, 1),
            Field("supportDomain", "תחום תמיכה", FieldKind.CodeList, "domains", ["in"], true, 2),
            Field("status", "סטטוס", FieldKind.CodeList, "statuses", ["in"], false, 3),
            Field("district", "מחוז", FieldKind.CodeList, "districts", ["in"], true, 4),
            Field("supportYear", "שנת תמיכה", FieldKind.YearRange, null, ["range", "single"], true, 5)
        ]);

    public static SearchMetadata SearchMetadata { get; } =
        new(Snapshot, new HashSet<string> { "culture-sport-admin", "welfare-admin" });

    public static ISearchMetadataProvider Provider { get; } = new StubProvider();

    private static FilterFieldRegistryEntry Field(
        string id, string label, string kind, string? referenceList,
        IReadOnlyList<string> operators, bool segmentable, int sortOrder) => new()
    {
        Id = id, Label = label, Kind = kind, ReferenceList = referenceList,
        Operators = operators, Segmentable = segmentable, SortOrder = sortOrder
    };

    private sealed class StubProvider : ISearchMetadataProvider
    {
        public Task<SearchMetadata> Get(CancellationToken ct = default) => Task.FromResult(SearchMetadata);
    }
}
