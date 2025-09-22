using Core.Db;
using Core.Models.Action;
using Core.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Common;

public class CommonService(IDbContextFactory<ApplicationDbContext> _dbContextFactory)
{
    public IQueryable<Case> GetCasesAsync(ApplicationDbContext db)
    {
        return db.Cases.AsQueryable();
    }

    public async Task CreateCaseAsync(string caseName, string description)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            dbContext.Cases.Add(new Case { Name = caseName, Description = description });
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateCaseAsync(string caseName, string? description = null, string? newName = null)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var originalCase = GetCasesAsync(dbContext).Single(x => x.Name == caseName);
            
            if (description != null)
                originalCase.Description = description;

            if (newName != null)
                originalCase.Name = newName;

            dbContext.Cases.Update(originalCase);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteCaseAsync(string caseName)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var originalCase = GetCasesAsync(dbContext).Single(x => x.Name == caseName);

            dbContext.Cases.Remove(originalCase);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<string, List<MainMenuItem>>> GetMainMenuItem(ApplicationDbContext db, string caseName)
    {
        return await db.MainMenuItems.Include(x => x.Case).Where(x => x.Case.Name == caseName).GroupBy(x => x.Category).ToDictionaryAsync(g => g.Key, g => g.ToList());
    }

    public async Task CreateMainMenuItem(string caseName, string name, string category, string topTag)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            dbContext.MainMenuItems.Add(new MainMenuItem { CaseId = (await dbContext.Cases.SingleAsync(x => x.Name == caseName)).Id, Name = name, Category = category, TopTag = topTag });
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateMainMenuItem(string caseName, string name, string? category = null, string? topTag = null, string? newName = null)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var originalMainMenuItem = await dbContext.MainMenuItems.Include(x => x.Case).SingleAsync(x => x.Case.Name == caseName && x.Name == name);

            if (category != null)
                originalMainMenuItem.Category = category;

            if (topTag != null)
                originalMainMenuItem.TopTag = topTag;

            if (newName != null)
                originalMainMenuItem.Name = newName;

            dbContext.MainMenuItems.Update(originalMainMenuItem);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteMainMenuItem(string caseName, string name)
    {
        using (var dbContext = await _dbContextFactory.CreateDbContextAsync())
        {
            var originalMainMenuItem = await dbContext.MainMenuItems.Include(x => x.Case).SingleAsync(x => x.Case.Name == caseName && x.Name == name);

            dbContext.MainMenuItems.Remove(originalMainMenuItem);
            await dbContext.SaveChangesAsync();
        }
    }
}
