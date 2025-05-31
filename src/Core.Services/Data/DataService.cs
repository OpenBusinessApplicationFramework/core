using Core.Db;
using Core.Models.Data;
using Core.Services.Action;
using Microsoft.EntityFrameworkCore;
using ValueType = Core.Models.Data.ValueType;

namespace Core.Services.Data;

public class DataService(IDbContextFactory<ApplicationDbContext> _dbContextFactory, ActionService _actionService)
{
    public IQueryable<DataDefinition>? GetDataDefinitions(ApplicationDbContext db, string caseName, string? definitionName = null)
    {
        var query = db.DataDefinitions.Include(d => d.DataEntries).ThenInclude(e => e.Tags).Include(d => d.DataEntries).ThenInclude(e => e.DataSet).Include(d => d.Case).Where(d => d.Case.Name == caseName);

        if (!string.IsNullOrWhiteSpace(definitionName))
            query = query.Where(d => d.Name == definitionName);

        return query;
    }

    public async Task<DataDefinition> CreateDataDefinitionAsync(string caseName, DataDefinition newDefinition)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var caseEntity = await db.Cases.FirstOrDefaultAsync(c => c.Name == caseName);
        if (caseEntity == null)
            throw new InvalidOperationException($"Case '{caseName}' not found.");

        newDefinition.Case = caseEntity;

        await db.DataDefinitions.AddAsync(newDefinition);
        await db.SaveChangesAsync();
        return newDefinition;
    }

    public async Task<DataDefinition> UpdateDataDefinitionAsync(string caseName, DataDefinition updatedDefinition)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var existing = await db.DataDefinitions.Include(d => d.Case).FirstOrDefaultAsync(d => d.Case.Name == caseName && d.Id == updatedDefinition.Id);

        if (existing == null)
            throw new InvalidOperationException($"DataDefinition '{updatedDefinition.Name}' not found in case '{caseName}'.");

        existing.Name = updatedDefinition.Name;
        existing.MultipleValues = updatedDefinition.MultipleValues;
        existing.ReadOnly = updatedDefinition.ReadOnly;
        existing.AutoIncrease = updatedDefinition.AutoIncrease;
        existing.InitialValue = updatedDefinition.InitialValue;
        existing.ValueType = updatedDefinition.ValueType;
        existing.ActionForCalculated = updatedDefinition.ActionForCalculated;
        existing.CalculateType = updatedDefinition.CalculateType;
        existing.ConnectionType = updatedDefinition.ConnectionType;
        existing.PathForConnected = updatedDefinition.PathForConnected;

        db.DataDefinitions.Update(existing);
        await db.SaveChangesAsync();
        return existing;
    }

    public async Task DeleteDataDefinitionAsync(string caseName, long definitionId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var existing = await db.DataDefinitions.Include(d => d.Case).FirstOrDefaultAsync(d => d.Case.Name == caseName && d.Id == definitionId);

        if (existing == null)
            throw new InvalidOperationException($"DataDefinition with ID {definitionId} not found in case '{caseName}'.");

        db.DataDefinitions.Remove(existing);
        await db.SaveChangesAsync();
    }

    public async Task<List<DataEntry>> GetDataEntriesAsync(string caseName, string dataDefinitionName, string[]? tags = null, string[]? dataSetNames = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var query = db.DataEntries.Include(e => e.Case).Include(e => e.DataDefinition).Include(e => e.Tags).Include(e => e.DataSet).Where(e => e.Case.Name == caseName && e.DataDefinition.Name == dataDefinitionName);

        if (tags != null && tags.Length > 0)
            query = query.Where(e => e.Tags.Any(t => tags.Contains(t.Name)));

        if (dataSetNames != null && dataSetNames.Length > 0)
            query = query.Where(e => dataSetNames.Contains(e.DataSet.Name));

        var results = await query.ToListAsync();

        foreach (var result in results)
        {
            if (!result.DataDefinition.IsValid)
                throw new InvalidOperationException($"DataDefinition '{result.DataDefinition.Name}' is not valid.");

            object parsed = result.DataDefinition.ValueType switch
            {
                ValueType.Static => ParseValueInRightObject(result),
                ValueType.Calculated => await HandleCalculatedAsync(result),
                ValueType.Connected => HandleConnected(db, result),
                _ => throw new InvalidOperationException("Unknown ValueType.")
            };

            if (result.DataDefinition.MultipleValues)
                result.Values = (List<string>)parsed;
            else
                result.Value = (string)parsed;
        }

        return results;
    }

    public async Task<object> CreateDataEntryAsync(string caseName, string dataDefinitionName, List<string>? values, List<string>? tags = null, string? dataSetName = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var dataCase = await db.Cases.SingleAsync(c => c.Name == caseName);
        var def = await db.DataDefinitions.SingleAsync(d => d.Name == dataDefinitionName);
        DataSet? dataSet = null;
        if (!string.IsNullOrWhiteSpace(dataSetName))
            dataSet = await db.DataSets.SingleAsync(ds => ds.Name == dataSetName);

        var tagsToAdd = new List<Tag>();
        if (tags != null && tags.Count > 0)
            tagsToAdd = await db.Tags.Where(t => tags.Contains(t.Name)).ToListAsync();

        var entry = new DataEntry
        {
            Case = dataCase,
            DataDefinition = def,
            DataSet = dataSet,
            Tags = tagsToAdd,
            Values = string.IsNullOrWhiteSpace(def.InitialValue) ? (values ?? throw new Exception("Value is not allowed to be null")) : [def.InitialValue]
        };

        if (!def.IsValid)
            throw new InvalidOperationException($"DataDefinition '{def.Name}' is not valid.");

        if (!entry.IsValid)
            throw new InvalidOperationException("At least one tag or one dataset must be provided");

        db.DataEntries.Add(entry);
        await db.SaveChangesAsync();

        object parsed = ParseValueInRightObject(entry);
        if (def.CalculateType == CalculateType.OnInsert && !string.IsNullOrWhiteSpace(def.ActionForCalculated))
            await _actionService.ExecuteActionAsync(caseName, def.ActionForCalculated, parsed);

        return parsed;
    }

    public async Task<object> UpdateDataEntryAsync(long entryId, List<string> values, List<string>? tags = null, string? dataSetName = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var entry = await db.DataEntries.Include(e => e.DataDefinition).Include(e => e.Tags).Include(e => e.DataSet).Include(e => e.Case).SingleOrDefaultAsync(e => e.Id == entryId) ?? throw new KeyNotFoundException($"DataEntry with id {entryId} not found.");

        var def = entry.DataDefinition;
        entry.Values = values.ToList();

        if (tags != null)
        {
            var newTags = await db.Tags.Where(t => tags.Contains(t.Name)).ToListAsync();
            entry.Tags = newTags;
        }

        if (!string.IsNullOrWhiteSpace(dataSetName))
            entry.DataSet = await db.DataSets.SingleAsync(ds => ds.Name == dataSetName);

        if (!def.IsValid)
            throw new InvalidOperationException($"DataDefinition '{def.Name}' is not valid.");

        if (!entry.IsValid)
            throw new InvalidOperationException("At least one tag or one dataset must be provided");

        await db.SaveChangesAsync();

        object parsed = ParseValueInRightObject(entry);
        if (def.CalculateType == CalculateType.OnInsert && !string.IsNullOrWhiteSpace(def.ActionForCalculated))
            await _actionService.ExecuteActionAsync(entry.Case.Name, def.ActionForCalculated, parsed);

        return parsed;
    }

    public async Task DeleteDataEntryAsync(long entryId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var entry = await db.DataEntries.FindAsync(entryId) ?? throw new KeyNotFoundException($"DataEntry with id {entryId} not found.");

        db.DataEntries.Remove(entry);
        await db.SaveChangesAsync();
    }

    private async Task<object> HandleCalculatedAsync(DataEntry entry)
    {
        var def = entry.DataDefinition;
        var parsed = ParseValueInRightObject(entry);

        if (def.CalculateType == CalculateType.OnCall && !string.IsNullOrWhiteSpace(def.ActionForCalculated))
            await _actionService.ExecuteActionAsync(entry.Case.Name, def.ActionForCalculated, parsed);

        return parsed;
    }

    private object HandleConnected(ApplicationDbContext db, DataEntry entry)
    {
        var path = entry.DataDefinition.PathForConnected!.Split('.');
        var linked = db.DataEntries.Include(e => e.DataDefinition).Include(e => e.Tags).Include(e => e.DataSet).Where(e => e.Case.Name == path[0] && e.DataDefinition.Name == path[1]).ToList();

        if (linked.Count > 1)
            throw new InvalidOperationException("Too many linked entries were found.");
        if (!linked.Any())
            throw new InvalidOperationException($"Link '{entry.DataDefinition.PathForConnected}' not found.");

        var target = linked.First();
        if (!target.DataDefinition.IsValid)
            throw new InvalidOperationException($"Linked entry '{entry.DataDefinition.PathForConnected}' is invalid.");

        return ParseValueInRightObject(target);
    }

    private object ParseValueInRightObject(DataEntry entry) => entry.DataDefinition.MultipleValues ? entry.Values : entry.Values.FirstOrDefault()!;
}