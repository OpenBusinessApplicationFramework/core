using Core.Db;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Common;

public class CommonService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
{
    public async Task CreateTenantAsync(string tenantName)
    {
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            dbContext.Tenants.Add(new Models.Common.Tenant { Name = tenantName });
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task CreateCaseAsync(string caseName, string description, string tenantName)
    {
        using (var dbContext = await dbContextFactory.CreateDbContextAsync())
        {
            dbContext.Cases.Add(new Models.Common.Case { Name = caseName, Description = description, Tenant = await dbContext.Tenants.FirstAsync(t => t.Name == tenantName) });
            await dbContext.SaveChangesAsync();
        }
    }
}
