using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Persistence.Interfaces;
using SupportPlatform.Infrastructure.Repositories;

namespace SupportPlatform.Infrastructure;

/// <summary>Composition root for the Infrastructure layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SupportPlatformDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("SqlServer")));

        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IMetadataRepository, MetadataRepository>();

        return services;
    }
}
