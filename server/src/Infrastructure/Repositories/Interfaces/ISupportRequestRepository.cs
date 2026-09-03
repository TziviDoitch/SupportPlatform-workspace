using SupportPlatform.Domain.Entities;

namespace SupportPlatform.Infrastructure.Repositories.Interfaces;

/// <summary>
/// The support-request read seam for the search engine. Returns an <see cref="IQueryable{T}"/> so
/// <see cref="Search.DynamicQueryBuilder"/> can compose the whitelisted filters onto it; the tenant
/// global query filter still applies. Replaces the direct <c>DbContext</c> injection the S2
/// executor carried.
/// </summary>
public interface ISupportRequestRepository
{
    IQueryable<SupportRequest> Query();
}
