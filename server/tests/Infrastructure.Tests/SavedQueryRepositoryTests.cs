using SupportPlatform.Domain.Entities;
using SupportPlatform.Infrastructure.Repositories;

namespace SupportPlatform.Infrastructure.Tests;

public class SavedQueryRepositoryTests
{
    private static SavedQuery Row(string owner, string tenant, string name) => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        DefinitionJson = "{}",
        DefinitionHash = "sha256:x",
        OwnerUsername = owner,
        TenantId = tenant,
        CreatedAt = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task List_and_Find_are_scoped_to_owner_and_tenant()
    {
        using var testDb = new TestDb();
        var repo = new SavedQueryRepository(testDb.Context);

        var mine = Row("sarah", "culture-sport-admin", "mine");
        await repo.Add(mine, default);
        await repo.Add(Row("dan", "culture-sport-admin", "other owner"), default);
        await repo.Add(Row("sarah", "welfare-admin", "other tenant"), default);
        await repo.Save();

        var list = await repo.List("sarah", "culture-sport-admin");

        Assert.Equal(new[] { "mine" }, list.Select(q => q.Name));
        Assert.NotNull(await repo.Find(mine.Id, "sarah", "culture-sport-admin"));
        Assert.Null(await repo.Find(mine.Id, "dan", "culture-sport-admin"));
    }

    [Fact]
    public async Task Remove_deletes_only_the_target_row()
    {
        using var testDb = new TestDb();
        var repo = new SavedQueryRepository(testDb.Context);

        var a = Row("sarah", "culture-sport-admin", "a");
        var b = Row("sarah", "culture-sport-admin", "b");
        await repo.Add(a, default);
        await repo.Add(b, default);
        await repo.Save();

        await repo.Remove(a);
        await repo.Save();

        var list = await repo.List("sarah", "culture-sport-admin");
        Assert.Equal(new[] { "b" }, list.Select(q => q.Name));
    }
}
