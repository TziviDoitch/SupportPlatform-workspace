using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application;
using SupportPlatform.Application.NlQuery;
using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.NlQuery.RuleBased;
using SupportPlatform.Application.Search.Interfaces;

namespace SupportPlatform.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_registers_without_error_and_is_chainable()
    {
        var services = new ServiceCollection();

        var result = services.AddApplication();

        Assert.Same(services, result);
    }

    [Fact]
    public void The_nl_provider_resolves_to_the_rule_based_one()
    {
        var descriptor = Assert.Single(
            new ServiceCollection().AddApplication(), d => d.ServiceType == typeof(INlQueryProvider));

        Assert.Equal(typeof(RuleBasedNlQueryProvider), descriptor.ImplementationType);
    }

    [Fact]
    public void The_nl_use_case_never_executes_a_search()
    {
        // Parsing hands the definition back for review; POST /api/search stays the execution path.
        var dependencies = typeof(NlQueryService).GetConstructors().Single().GetParameters();

        Assert.DoesNotContain(dependencies, p => p.ParameterType == typeof(ISearchService));
    }
}
