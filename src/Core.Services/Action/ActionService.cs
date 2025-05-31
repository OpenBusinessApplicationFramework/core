using Core.Db;
using Core.Models.Action;
using Jint;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Action;

public class ActionService(IDbContextFactory<ApplicationDbContext> _dbContextFactory)
{
    public async Task ExecuteActionAsync(string caseName, string actionName, object? calculatedData = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var actionDef = await db.ActionDefinitions.Include(a => a.Case).SingleOrDefaultAsync(a => a.Case.Name == caseName && a.Name == actionName);

        if (actionDef == null)
            throw new InvalidOperationException($"Action '{actionName}' in case '{caseName}' not found.");

        var engine = new Engine();

        using (var http = new HttpClient())
            engine.Execute(await http.GetStringAsync("https://cdn.jsdelivr.net/npm/mathjs@14.4.0/lib/browser/math.min.js"));

        if (calculatedData != null)
            engine.SetValue("CalculatedDataEntry", calculatedData);

        foreach (var tagName in actionDef.TagUsedInAction)
        {
            if (tagName.Equals("math", StringComparison.OrdinalIgnoreCase) || tagName.Equals("CalculatedDataEntry", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Tag '{tagName}' is reserved and can't be used.");

            var entriesByTag = await db.DataEntries.Include(e => e.DataDefinition).Include(e => e.Tags).Where(e => e.Case.Name == caseName && e.Tags.Any(t => t.Name == tagName)).ToListAsync();
            var groupedByDef = entriesByTag.GroupBy(e => e.DataDefinition.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groupedByDef)
            {
                var defName = group.Key;
                var entries = group.ToList();
                engine.SetValue($"tag_{defName}", entries);
            }
        }

        foreach (var dsName in actionDef.DataSetUsedInAction)
        {
            if (dsName.Equals("math", StringComparison.OrdinalIgnoreCase) || dsName.Equals("CalculatedDataEntry", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Dataset name '{dsName}' is reserved and can't be used.");

            var entriesBySet = await db.DataEntries.Include(e => e.DataDefinition).Include(e => e.DataSet).Where(e => e.Case.Name == caseName && e.DataSet.Name == dsName).ToListAsync();

            foreach (var entry in entriesBySet)
            {
                var defName = entry.DataDefinition.Name;
                engine.SetValue($"set_{defName}", entry);
            }
        }

        try
        {
            engine.Execute(actionDef.ActionFunction);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error executing '{actionName}': {ex.Message}", ex);
        }

        await db.SaveChangesAsync();
    }

    public IQueryable<ActionDefinition> GetAllAsync(ApplicationDbContext db, string caseName)
    {
        return db.ActionDefinitions.Include(a => a.Case).Where(a => a.Case.Name == caseName).AsQueryable();
    }

    public async Task CreateAsync(string caseName, string name, string actionFunction, List<string> tagUsedInAction, List<string> dataSetUsedInAction)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var actionDef = new ActionDefinition()
        {
            CaseId = (await db.Cases.SingleAsync(x => x.Name == caseName)).Id,
            Name = name,
            ActionFunction = actionFunction,
            TagUsedInAction = tagUsedInAction,
            DataSetUsedInAction = dataSetUsedInAction
        };

        db.ActionDefinitions.Add(actionDef);
        await db.SaveChangesAsync();
    }

    public async Task<bool> UpdateAsync(string caseName, string name, string actionFunction, List<string> tagUsedInAction, List<string> dataSetUsedInAction, string? newName = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var exists = await db.ActionDefinitions.Include(x => x.Case).Where(x => x.Case.Name == caseName).SingleOrDefaultAsync(a => a.Name == name);
        if (exists == null)
            return false;

        if (newName != null)
            exists.Name = newName;

        exists.ActionFunction = actionFunction;
        exists.TagUsedInAction = tagUsedInAction;
        exists.DataSetUsedInAction = dataSetUsedInAction;

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
