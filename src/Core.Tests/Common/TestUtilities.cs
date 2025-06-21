using Core.Db;
using Microsoft.EntityFrameworkCore;

namespace Core.Tests.Common;

public static class TestUtilities
{
    public static (ApplicationDbContext Context, TestDbContextFactory Factory) CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ApplicationDbContext(options);
        TestDataSeeder.SeedAsync(context).GetAwaiter().GetResult();
        var factory = new TestDbContextFactory(options);
        return (context, factory);
    }
}