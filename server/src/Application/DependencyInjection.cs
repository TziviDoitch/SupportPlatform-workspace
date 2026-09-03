using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
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

/// <summary>Composition root for the Application layer.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMetadataService, MetadataService>();

        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IValidator<QueryDefinition>, QueryDefinitionValidator>();
        services.AddSingleton<QuestionTextRenderer>();

        services.AddScoped<ISavedQueryService, SavedQueryService>();

        // The AI seam (DESIGN_QA Q6): swapping the provider is this one line.
        services.AddScoped<INlQueryService, NlQueryService>();
        services.AddScoped<INlQueryProvider, RuleBasedNlQueryProvider>();

        return services;
    }
}
