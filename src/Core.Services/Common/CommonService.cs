using Core.Db;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Common;

public class CommonService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task CreateCaseAsync(string caseName, string description, string tenantName)
    {
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            dbContext.Cases.Add(new Models.Common.Case { Name = caseName, Description = description });
            await dbContext.SaveChangesAsync();
        }
    }
}
