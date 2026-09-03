using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.NlQuery.RuleBased.Rules;
using SupportPlatform.Application.Search;

namespace SupportPlatform.Application.NlQuery.RuleBased;

/// <summary>
/// The PoC provider: a deterministic parser over a deliberately limited subset of Hebrew. The
/// same question always yields the same <see cref="QueryDefinition"/>, and every value it emits
/// came from the metadata — when a word cannot be mapped it is reported in
/// <see cref="NlParseResult.Unresolved"/>, never replaced by a guess.
///
/// It stays thin on purpose: build the text, run the rules, assemble the definition. The matching
/// lives in <c>Rules/</c>.
/// </summary>
public sealed class RuleBasedNlQueryProvider : INlQueryProvider
{
    public Task<NlParseResult> Parse(
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

        return Task.FromResult(new NlParseResult(definition, question.Coverage(), question.Unclaimed()));
    }

    /// <summary>
    /// Words that named a field the parser actually used — "בתחום" in "בתחום התרבות", "בשנת" in
    /// "בשנת 2024". They were understood, so they must not surface as unresolved.
    ///
    /// Only <paramref name="resolved"/> fields count. A field named in the question but not used —
    /// "לפי סטטוס", where status is not segmentable — stays unclaimed, so the user is told the
    /// grouping was dropped instead of getting a full-confidence parse that silently ignored it.
    /// </summary>
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
