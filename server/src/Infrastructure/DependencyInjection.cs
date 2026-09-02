using Microsoft.Extensions.DependencyInjection;

namespace SupportPlatform.Infrastructure;

/// <summary>Composition root for the Infrastructure layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // DbContext, repositories and provider implementations are registered here from S1 onward.
        return services;
    }
}
