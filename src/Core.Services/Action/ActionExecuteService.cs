using Core.Db;
using Core.Models.Data;
using Core.Services.Data;
using Jint;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Core.Services.Action;

public class ActionExecuteService(IDbContextFactory<ApplicationDbContext> _dbContextFactory, DataService _dataService, DataAnnotationService _dataAnnotationService)
{
    public async Task ExecuteActionAsync(string caseName, string actionName, long? entryIdToCalculate = null, List<string>? tagArguments = null, Dictionary<string, string>? arguments = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var actionDef = await db.ActionDefinitions.Include(a => a.Case).SingleOrDefaultAsync(a => a.Case.Name == caseName && a.Name == actionName);

        if (actionDef == null)
            throw new InvalidOperationException($"Action '{actionName}' in case '{caseName}' not found.");

        var engine = new Engine();

        using (var http = new HttpClient())
            engine.Execute(await http.GetStringAsync("https://cdn.jsdelivr.net/npm/mathjs@14.4.0/lib/browser/math.min.js"));

        bool skipCalculatedAtEntriesLoad = false;

        if (entryIdToCalculate != null)
        {
            var entry = await db.DataEntries.Include(e => e.Case).Include(e => e.DataDefinition).SingleAsync(x => x.Case.Name == caseName && x.Id == entryIdToCalculate);
            engine.SetValue("CalculatedDataEntry", entry);

            skipCalculatedAtEntriesLoad = entry.DataDefinition.CalculateType == CalculateType.OnCall;
        }

        if (arguments != null && arguments.Any())
        {
            foreach (var item in arguments)
            {
                if (!actionDef.ValueViaArgument!.Contains(item.Key))
                    throw new Exception($"ValueViaArgument doesn't contain {item.Key}");

                engine.SetValue(item.Key, item.Value);
            }
        }

        var allArgumentsFromTags = actionDef.TagUsedInAction ?? new List<string>();
        if (tagArguments != null)
        {
            foreach (var argument in tagArguments)
            {
                string topTagName = argument.Split("_")[argument.Split("_").Length - 2];
                if (!(actionDef.TagViaArgument!.Contains(argument) || actionDef.TagViaArgument!.Contains(topTagName)))
                    throw new Exception($"TagViaArgument doesn't contain {argument}");

                allArgumentsFromTags.Add(argument);
            }
        }
            

        var allEntries = (await _dataService.GetDataEntriesAsync(db, caseName, null, allArgumentsFromTags.ToArray(), skipCalculated: skipCalculatedAtEntriesLoad)).results.ToList();

        foreach (var tagName in allArgumentsFromTags)
        {
            if (tagName.Equals("math", StringComparison.OrdinalIgnoreCase) || tagName.Equals("CalculatedDataEntry", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Dataset name '{tagName}' is reserved and can't be used.");

            var tag = await _dataAnnotationService.GetTags(await _dbContextFactory.CreateDbContextAsync(), caseName)!.SingleAsync(x => x.Name == tagName);
            if (tag.AllowedActions == null || !tag.AllowedActions.Contains(actionName))
            {
                string topTagName = tagName.Split("_")[tagName.Split("_").Length - 2];
                var topTag = await _dataAnnotationService.GetTags(await _dbContextFactory.CreateDbContextAsync(), caseName)!.SingleAsync(x => x.Name == topTagName);
                if (topTag.AllowedSubActions == null || !topTag.AllowedSubActions.Contains(actionName))
                    throw new InvalidOperationException($"Tag {tagName} / {topTagName} is not allowed to be executed in action {actionName}.");
            }

            var entriesByTag = allEntries.Where(e => e.Tags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))).ToList();

            var grouped = entriesByTag.GroupBy(e => e.DataDefinition.Name, StringComparer.OrdinalIgnoreCase);

            foreach (var grp in grouped)
                engine.SetValue($"{tagName}_{grp.Key}", grp.ToList());
        }

        try
        {
            engine.Execute(actionDef.ActionFunction);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error executing '{actionName}': {ex.Message}", ex);
        }

        var modified = db.ChangeTracker.Entries<DataEntry>().Where(e => e.State == EntityState.Modified).Select(e => e.Entity).ToList();

        foreach (var entry in modified)
        {
            await _dataService.UpdateDataEntryAsync(caseName, entry.Id, entry.Values, entry.Tags?.Select(t => t.Name).ToList() ?? null, entry.Id == (entryIdToCalculate ?? 0));
        }
    }
}
