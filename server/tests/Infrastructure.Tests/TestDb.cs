using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SupportPlatform.Infrastructure.Persistence;
using SupportPlatform.Infrastructure.Persistence.Interfaces;

namespace SupportPlatform.Infrastructure.Tests;

/// <summary>
/// An isolated in-memory SQLite database. Holds the connection open for the fixture's lifetime
/// (an in-memory SQLite db is dropped when its last connection closes).
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDb(ITenantContext? tenant = null)
    {
        Tenant = tenant as MutableTenantContext ?? new MutableTenantContext();
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<SupportPlatformDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new SupportPlatformDbContext(options, Tenant);
        Context.Database.EnsureCreated();
    }

    public SupportPlatformDbContext Context { get; }

    public MutableTenantContext Tenant { get; }

    /// <summary>A fresh context over the same database — used to read back what another context wrote.</summary>
    public SupportPlatformDbContext NewContext(ITenantContext? tenant = null)
    {
        var options = new DbContextOptionsBuilder<SupportPlatformDbContext>()
            .UseSqlite(_connection)
            .Options;
        return new SupportPlatformDbContext(options, tenant ?? Tenant);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
