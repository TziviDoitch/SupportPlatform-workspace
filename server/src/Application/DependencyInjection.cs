using Microsoft.Extensions.DependencyInjection;

namespace SupportPlatform.Application;

/// <summary>Composition root for the Application layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Use-case services and validators are registered here from S1 onward.
        return services;
    }
}
