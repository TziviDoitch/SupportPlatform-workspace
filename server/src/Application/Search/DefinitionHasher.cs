using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SupportPlatform.Application.Search;

/// <summary>
/// Canonical SHA-256 of a <see cref="QueryDefinition"/> — filter keys, filter codes and metrics
/// are ordered so logically equal definitions hash the same, while order-significant lists
/// (<c>segmentation</c>, <c>sort</c>) are kept as-is. Feeds <c>executionMeta.definitionHash</c>
/// and is the key for the S5 search cache.
/// </summary>
public static class DefinitionHasher
{
    public static string Hash(QueryDefinition def)
    {
        var canonical = new
        {
            tenantId = def.TenantId,
            filters = def.Filters.OrderBy(f => f.Key, StringComparer.Ordinal)
                .Select(f => new { field = f.Key, value = Canonical(f.Value) }),
            segmentation = def.Segmentation,
            metrics = def.EffectiveMetrics.OrderBy(m => m, StringComparer.Ordinal),
            paging = new { def.Paging.PageSize, def.Paging.PageNumber },
            sort = def.Sort.Select(s => new { s.Field, s.Direction })
        };

        var json = JsonSerializer.Serialize(canonical);
        var digest = SHA256.HashData(Encoding.UTF8.GetBytes(json));
        return "sha256:" + Convert.ToHexString(digest).ToLowerInvariant();
    }

    private static object Canonical(FilterValue value) => value switch
    {
        FilterValue.Codes c => new { kind = "codes", values = c.Values.OrderBy(v => v, StringComparer.Ordinal) },
        FilterValue.YearRange r => (object)new { kind = "range", r.From, r.To },
        FilterValue.YearSingle s => new { kind = "single", s.Value },
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
}
