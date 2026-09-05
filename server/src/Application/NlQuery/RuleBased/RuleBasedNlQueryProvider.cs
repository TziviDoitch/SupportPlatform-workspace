using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.NlQuery.RuleBased.Rules;
using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.RuleBased;

// PoC provider: a deterministic parser over a deliberately limited subset of Hebrew. The same
// question always yields the same QueryDefinition, and every value came from the metadata — an
// unmappable word is reported as unresolved, never guessed. Build the text, run the rules,
// assemble the definition; the matching lives in Rules/.
public sealed class RuleBasedNlQueryProvider : INlQueryProvider
{
    public const string ProviderKey = "ruleBased";

    public Task<NlTranslation> Translate(
        string text, string tenantId, SearchMetadata metadata, CancellationToken ct = default)
    {
        var question = new NlText(text);
        var filters = new Dictionary<string, FilterValue>();

        CodeListFilterRule.Apply(question, metadata, filters);
        YearRule.Apply(question, metadata, filters);
        var segmentation = SegmentationRule.Apply(question, metadata);
        ClaimFieldNames(question, metadata, filters.Keys.Concat(segmentation));

        var definition = new QueryDefinition
        {
            TenantId = tenantId,
            Filters = filters,
            Segmentation = segmentation,
            Metrics = [Metric.Count, Metric.SumAmountApproved]
        };

        return Task.FromResult(new NlTranslation(definition, question.Coverage(), question.Unclaimed()));
    }

    // Claims words that named a field the parser actually used ("בתחום" in "בתחום התרבות"), so
    // they don't surface as unresolved. A field named but not used ("לפי סטטוס", not segmentable)
    // stays unclaimed, so the user is told the grouping was dropped.
    private static void ClaimFieldNames(NlText question, SearchMetadata metadata, IEnumerable<string> resolved)
    {
        var words = resolved
            .Select(metadata.Field)
            .Where(field => field is not null)
            .SelectMany(field => HebrewText.Stems(field!.Label))
            .Distinct();

        foreach (var word in words)
            while (question.TryClaim([word]))
            {
                // Every occurrence — a field may be named more than once.
            }
    }
}
