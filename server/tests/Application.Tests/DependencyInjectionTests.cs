using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application;
using SupportPlatform.Application.NlQuery;
using SupportPlatform.Application.NlQuery.Interfaces;
using SupportPlatform.Application.NlQuery.RuleBased;
using SupportPlatform.Application.Search;
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
    public void The_default_configuration_resolves_the_rule_based_provider()
    {
        using var provider = new ServiceCollection().AddApplication().BuildServiceProvider();

        Assert.IsType<RuleBasedNlQueryProvider>(Resolve(provider));
    }

    [Fact]
    public void Configuration_selects_a_provider_registered_under_another_key()
    {
        // The point of the seam: implement a provider, register it under a key, name that key in
        // NlQuery:Provider — no other type changes (DESIGN_QA §6).
        using var provider = new ServiceCollection()
            .AddApplication()
            .AddKeyedScoped<INlQueryProvider, FakeNlQueryProvider>(FakeNlQueryProvider.ProviderKey)
            .AddSingleton(new NlQueryOptions { Provider = FakeNlQueryProvider.ProviderKey })
            .BuildServiceProvider();

        Assert.IsType<FakeNlQueryProvider>(Resolve(provider));
    }

    [Fact]
    public void An_unknown_provider_key_fails_with_a_message_naming_it()
    {
        using var provider = new ServiceCollection()
            .AddApplication()
            .AddSingleton(new NlQueryOptions { Provider = "gemini" })
            .BuildServiceProvider();

        var error = Assert.Throws<InvalidOperationException>(() => { Resolve(provider); });

        Assert.Contains("gemini", error.Message);
        Assert.Contains(RuleBasedNlQueryProvider.ProviderKey, error.Message);
    }

    [Fact]
    public void The_nl_use_case_never_executes_a_search()
    {
        // Parsing hands the definition back for review; POST /api/search stays the execution path.
        var dependencies = typeof(NlQueryService).GetConstructors().Single().GetParameters();

        Assert.DoesNotContain(dependencies, p => p.ParameterType == typeof(ISearchService));
    }

    private static INlQueryProvider Resolve(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<INlQueryProvider>();
    }

    /// <summary>Stands in for a future LLM-backed provider — only its registration matters here.</summary>
    private sealed class FakeNlQueryProvider : INlQueryProvider
    {
        public const string ProviderKey = "fake";

        public Task<NlTranslation> Translate(
            string text, string tenantId, SearchMetadata metadata, CancellationToken ct = default) =>
            throw new NotSupportedException();
    }
}
