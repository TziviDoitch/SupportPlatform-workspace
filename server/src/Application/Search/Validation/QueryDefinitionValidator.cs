using FluentValidation;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.Search.Validation;

/// <summary>
/// Runtime validation of a <see cref="QueryDefinition"/> against the registry whitelist and the
/// known tenants (<c>docs/contracts/error-model.md</c> — the 400 <c>validation</c> catalogue).
/// Structural rules already covered by the JSON schema are not repeated here.
/// </summary>
public sealed class QueryDefinitionValidator : AbstractValidator<QueryDefinition>
{
    private static readonly string[] Directions = ["asc", "desc"];

    public QueryDefinitionValidator(ISearchMetadataProvider metadata)
    {
        RuleFor(d => d).CustomAsync(async (def, ctx, ct) =>
        {
            var meta = await metadata.Get(ct);

            if (string.IsNullOrWhiteSpace(def.TenantId))
                ctx.AddFailure("tenantId", "tenantId is required.");
            else if (!meta.TenantIds.Contains(def.TenantId))
                ctx.AddFailure("tenantId", $"Unknown tenant '{def.TenantId}'.");

            foreach (var (fieldId, value) in def.Filters)
                ValidateFilter(ctx, meta, fieldId, value);

            for (var i = 0; i < def.Segmentation.Count; i++)
            {
                var id = def.Segmentation[i];
                var entry = meta.Field(id);
                if (entry is null)
                    ctx.AddFailure($"segmentation[{i}]", $"'{id}' is not a known filter field.");
                else if (!entry.Segmentable)
                    ctx.AddFailure($"segmentation[{i}]", $"'{id}' is not segmentable.");
            }

            for (var i = 0; i < def.Metrics.Count; i++)
                if (!Metric.All.Contains(def.Metrics[i]))
                    ctx.AddFailure($"metrics[{i}]", $"'{def.Metrics[i]}' is not a known metric.");

            if (def.Paging.PageSize is < 1 or > 200)
                ctx.AddFailure("paging.pageSize", "pageSize must be between 1 and 200.");
            if (def.Paging.PageNumber < 1)
                ctx.AddFailure("paging.pageNumber", "pageNumber must be 1 or greater.");

            ValidateSort(ctx, def);
        });
    }

    private static void ValidateFilter(
        ValidationContext<QueryDefinition> ctx, SearchMetadata meta, string fieldId, FilterValue value)
    {
        var path = $"filters.{fieldId}";
        var entry = meta.Field(fieldId);
        if (entry is null)
        {
            ctx.AddFailure(path, $"'{fieldId}' is not a known filter field.");
            return;
        }

        switch (entry.Kind)
        {
            case FieldKind.CodeList when value is FilterValue.Codes c:
                if (c.Values.Count == 0 || c.Values.Any(string.IsNullOrWhiteSpace))
                    ctx.AddFailure(path, "Provide one or more non-empty codes.");
                break;
            case FieldKind.YearRange when value is FilterValue.YearRange r:
                if (r.From > r.To)
                    ctx.AddFailure(path, "'from' must be less than or equal to 'to'.");
                break;
            case FieldKind.YearRange when value is FilterValue.YearSingle:
                break;
            default:
                ctx.AddFailure(path, $"Value shape does not match field kind '{entry.Kind}'.");
                break;
        }
    }

    private static void ValidateSort(ValidationContext<QueryDefinition> ctx, QueryDefinition def)
    {
        for (var i = 0; i < def.Sort.Count; i++)
        {
            var spec = def.Sort[i];
            if (!Directions.Contains(spec.Direction))
                ctx.AddFailure($"sort[{i}].direction", "direction must be 'asc' or 'desc'.");

            var sortable = def.Segmentation.Contains(spec.Field) || Metric.All.Contains(spec.Field);
            if (!sortable)
                ctx.AddFailure($"sort[{i}].field",
                    $"'{spec.Field}' must be a segmentation field or a metric name.");
        }
    }
}
