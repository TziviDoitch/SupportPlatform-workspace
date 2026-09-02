using Microsoft.Extensions.DependencyInjection;
using SupportPlatform.Application;

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
}
