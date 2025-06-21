using Core.Db;
using Core.Models.Data;
using Core.Services.Data;
using Jint;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Core.Services.Action
{
    public class ActionExecuteService(IDbContextFactory<ApplicationDbContext> _dbContextFactory, DataService _dataService)
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

            var allEntries = (await _dataService.GetDataEntriesAsync(db, caseName, null, actionDef.TagUsedInAction.ToArray())).ToList();

            foreach (var tagName in actionDef.TagUsedInAction)
            {
                if (tagName.Equals("math", StringComparison.OrdinalIgnoreCase) || tagName.Equals("CalculatedDataEntry", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Dataset name '{tagName}' is reserved and can't be used.");
                }

                var entriesByTag = allEntries.Where(e => e.Tags.Any(t => t.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))).ToList();

                var grouped = entriesByTag.GroupBy(e => e.DataDefinition.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var grp in grouped)
                    engine.SetValue($"tag_{grp.Key}", grp.ToList());
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
                await _dataService.UpdateDataEntryAsync(caseName, entry.Id, entry.Values, entry.Tags?.Select(t => t.Name).ToList() ?? null);
            }
        }
    }
}
