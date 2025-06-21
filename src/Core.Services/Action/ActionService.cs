using Core.Db;
using Core.Models.Action;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Action;

public class ActionService(IDbContextFactory<ApplicationDbContext> _dbContextFactory)
{
    public IQueryable<ActionDefinition> GetAllAsync(ApplicationDbContext db, string caseName)
    {
        return db.ActionDefinitions.Include(a => a.Case).Where(a => a.Case.Name == caseName).AsQueryable();
    }

    public async Task CreateAsync(string caseName, string name, string actionFunction, List<string> tagUsedInAction, List<string> dataSetCategoryUsedInAction)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var actionDef = new ActionDefinition()
        {
            CaseId = (await db.Cases.SingleAsync(x => x.Name == caseName)).Id,
            Name = name,
            ActionFunction = actionFunction,
            TagUsedInAction = tagUsedInAction
        };

        db.ActionDefinitions.Add(actionDef);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(string caseName, string name, string actionFunction, List<string> tagUsedInAction, List<string> dataSetCategoryUsedInAction, string? newName = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var exists = await db.ActionDefinitions.Include(x => x.Case).Where(x => x.Case.Name == caseName).SingleOrDefaultAsync(a => a.Name == name);
        if (exists == null)
            return false;

        if (newName != null)
            exists.Name = newName;

        exists.ActionFunction = actionFunction;
        exists.TagUsedInAction = tagUsedInAction;

        db.Update(exists);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(string caseName, string name)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var entity = await db.ActionDefinitions.Include(x => x.Case).Where(x => x.Case.Name == caseName).SingleOrDefaultAsync(a => a.Name == name);
        if (entity == null)
            return false;

        db.ActionDefinitions.Remove(entity);
        await db.SaveChangesAsync();
        return true;
    }
}
