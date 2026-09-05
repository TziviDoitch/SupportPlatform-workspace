using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Repositories.Interfaces;

// The support-request read seam for the search engine. Returns an IQueryable so
// DynamicQueryBuilder can compose the whitelisted filters onto it; the tenant global query filter
// still applies.
public interface ISupportRequestRepository
{
    IQueryable<SupportRequest> Query();
}
