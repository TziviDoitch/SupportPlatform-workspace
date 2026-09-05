using Microsoft.EntityFrameworkCore;
using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Repositories.Interfaces;

namespace SupportPlatform.Infrastructure.Repositories;

public sealed class SupportRequestRepository(SupportPlatformDbContext db) : ISupportRequestRepository
{
    public IQueryable<SupportRequest> Query() => db.SupportRequests.AsNoTracking();
}
