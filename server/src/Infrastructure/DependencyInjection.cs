using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Persistence.Interfaces;
using SupportPlatform.Infrastructure.Repositories;
using SupportPlatform.Infrastructure.Search;
using SupportPlatform.Infrastructure.Search.Filters;
using SupportPlatform.Infrastructure.Search.Filters.Interfaces;

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

        services.AddScoped<ISearchMetadataProvider, SearchMetadataProvider>();
        services.AddScoped<ISearchQueryExecutor, SearchQueryExecutor>();
        services.AddScoped<DynamicQueryBuilder>();

        AddFilterHandlers(services);

        return services;
    }

    private static void AddFilterHandlers(IServiceCollection services)
    {
        foreach (var handler in FilterHandlers.Default)
            services.AddSingleton(handler);

        services.AddSingleton<IFilterHandlerResolver, FilterHandlerResolver>();
    }
}
