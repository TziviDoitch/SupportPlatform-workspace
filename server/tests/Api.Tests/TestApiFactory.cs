using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using SupportPlatform.Application.Search;
using SupportPlatform.Infrastructure.Persistence;

namespace SupportPlatform.Api.Tests;

/// <summary>
/// Boots the API against an isolated in-memory SQLite database, seeded once. Replaces the
/// SQL Server context registered by <c>AddInfrastructure</c>.
/// </summary>
public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<SupportPlatformDbContext>>();
            services.RemoveAll<SupportPlatformDbContext>();
            services.AddDbContext<SupportPlatformDbContext>(o => o.UseSqlite(_connection));

            // Deterministic endpoint tests: dedup off so repeated identical posts always re-run.
            // The cache itself is covered by SearchServiceTests.
            services.RemoveAll<SearchCacheOptions>();
            services.AddSingleton(new SearchCacheOptions { TtlSeconds = 0 });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SupportPlatformDbContext>();
        db.Database.EnsureCreated();
        DbSeeder.Seed(db);

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            _connection.Dispose();
    }
}
