using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application.Metadata;
using SupportPlatform.Application.Metadata.Interfaces;

namespace SupportPlatform.Application;

/// <summary>Composition root for the Application layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMetadataService, MetadataService>();
        return services;
    }
}
