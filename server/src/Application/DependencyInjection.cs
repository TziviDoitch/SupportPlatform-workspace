using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application.Identity;
using SupportPlatform.Application.Metadata;
using SupportPlatform.Application.Metadata.Interfaces;
using SupportPlatform.Application.NlQuery;
using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.NlQuery.RuleBased;
using SupportPlatform.Application.SavedQueries;
using SupportPlatform.Application.SavedQueries.Interfaces;
using SupportPlatform.Application.Search;
using SupportPlatform.Application.Search.Interfaces;
using SupportPlatform.Application.Search.Validation;

namespace SupportPlatform.Application;

public static class DependencyInjection
{
    // One entry per INlQueryProvider implementation, keyed by the value NlQuery:Provider selects.
    // Adding an LLM-backed provider is one entry here plus one configuration value.
    private static readonly Dictionary<string, Type> NlQueryProviders = new()
    {
        [RuleBasedNlQueryProvider.ProviderKey] = typeof(RuleBasedNlQueryProvider)
    };

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<TenantAccessGuard>();

        services.AddScoped<IMetadataService, MetadataService>();

        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IValidator<QueryDefinition>, QueryDefinitionValidator>();
        services.AddSingleton<QuestionTextRenderer>();

        services.AddScoped<ISavedQueryService, SavedQueryService>();

        services.AddScoped<INlQueryService, NlQueryService>();
        foreach (var (key, implementation) in NlQueryProviders)
            services.AddKeyedScoped(typeof(INlQueryProvider), key, implementation);
        services.AddScoped(ResolveNlQueryProvider);

        return services;
    }

    private static INlQueryProvider ResolveNlQueryProvider(IServiceProvider services)
    {
        // No options bound (a bare AddApplication in a test) means the type default, not a failure.
        var key = (services.GetService<NlQueryOptions>() ?? new NlQueryOptions()).Provider;

        return services.GetKeyedService<INlQueryProvider>(key)
               ?? throw new InvalidOperationException(
                   $"No INlQueryProvider is registered under key '{key}' (NlQuery:Provider). " +
                   $"Built-in providers: {string.Join(", ", NlQueryProviders.Keys)}.");
    }
}
