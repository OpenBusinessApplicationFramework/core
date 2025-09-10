using Core.Db;
using Core.Models.Data;
using Core.Services.Action;
using IdGen;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq;
using ValueType = Core.Models.Data.ValueType;

namespace Core.Services.Data;

public class DataService(IDbContextFactory<ApplicationDbContext> _dbContextFactory, DataAnnotationService _dataAnnotationService, ActionService _actionService)
{
    public IQueryable<DataDefinition>? GetDataDefinitions(ApplicationDbContext db, string caseName, string? definitionName = null)
    {
        var query = db.DataDefinitions.Include(d => d.Case).Where(d => d.Case.Name == caseName);

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

    public async Task<(IList<DataEntry> results, int count)> GetDataEntriesAsync(ApplicationDbContext db, string caseName, string? dataDefinitionName = null, string[]? tags = null, string? getSubTagsFromTopTag = null, string? globalFilter = null, bool skipCalculated = false, int? top = null, int? skip = null)
    {
        var query = db.DataEntries.Include(e => e.Case).Include(e => e.DataDefinition).Include(e => e.Tags).Where(e => e.Case.Name == caseName);

        if (!string.IsNullOrWhiteSpace(dataDefinitionName))
            query = query.Where(e => e.DataDefinition.Name == dataDefinitionName);

        if (tags?.Length > 0)
            query = query.Where(e => e.Tags.Any(t => tags.Contains(t.Name)));

        if (!string.IsNullOrWhiteSpace(globalFilter))
        {
            var tagNames = await query
                .Where(e => e.Values.Any(x => x.ToLower().StartsWith(globalFilter.ToLower())))
                .SelectMany(e => e.Tags.Select(t => t.Name))
                .Distinct()
                .ToListAsync();

            if (getSubTagsFromTopTag != null)
                tagNames = tagNames.Where(e => e.StartsWith($"{getSubTagsFromTopTag}_")).ToList();

            query = query.Where(e => e.Tags.Any(t => tagNames.Contains(t.Name)));
        }

        if (!string.IsNullOrWhiteSpace(getSubTagsFromTopTag))
            query = query.Where(e => e.Tags.Any(t => t.Name.StartsWith($"{getSubTagsFromTopTag}_")));

        var tagIdPageQuery = query.SelectMany(e => e.Tags.Where(t => t.UniqueDefinition)).Select(t => t.Id).Distinct().OrderBy(id => id);

        if (skip.HasValue) 
            tagIdPageQuery = tagIdPageQuery.Skip(skip.Value).OrderBy(id => id);
        if (top.HasValue) 
            tagIdPageQuery = tagIdPageQuery.Take(top.Value).OrderBy(id => id);

        var pageTagIds = await tagIdPageQuery.ToListAsync();
        List<DataEntry> results = new List<DataEntry>();
        int? totalCount = null;

        if (pageTagIds.Count == 0)
        {
            results = await query.ToListAsync();
            totalCount = results.Count;
        }
        else
        {
            IQueryable<DataEntry> finalQuery = query
                .Where(e => e.Tags.Any(t => t.UniqueDefinition && pageTagIds.Contains(t.Id)))
                .OrderBy(e => e.Tags
                    .Where(t => t.UniqueDefinition && pageTagIds.Contains(t.Id))
                    .Select(t => t.Id)
                    .Min())
                .ThenBy(e => e.Id);

            results = await finalQuery.ToListAsync();
            totalCount = await query.SelectMany(e => e.Tags.Where(t => t.UniqueDefinition)).Select(t => t.Id).Distinct().CountAsync();
        }

        foreach (var result in results)
        {
            if (!result.DataDefinition.IsValid)
                throw new InvalidOperationException($"DataDefinition '{result.DataDefinition.Name}' is not valid.");

            object parsed = result.DataDefinition.ValueType switch
            {
                ValueType.Static => ParseValueInRightObject(result),
                ValueType.Calculated => skipCalculated ? ParseValueInRightObject(result) : await HandleCalculatedAsync(result),
                ValueType.Connected => HandleConnected(await _dbContextFactory.CreateDbContextAsync(), result),
                ValueType.UniqueIdentifier => ParseValueInRightObject(result),
                ValueType.Select => ParseValueInRightObject(result),
                ValueType.SubGrid => ParseValueInRightObject(result),
                _ => throw new InvalidOperationException("Unknown ValueType.")
            };

            if (result.DataDefinition.MultipleValues)
                result.Values = (List<string>)parsed;
            else
                result.Value = (string)parsed;
        }

        return (results, totalCount ?? -1);
    }

    public async Task<(long id, object value)> CreateDataEntryAsync(string caseName, string dataDefinitionName, List<string>? values, List<string>? tags = null, bool? addAllowedTagsIfTopTagHasIt = false)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var dataCase = await db.Cases.SingleAsync(c => c.Name == caseName);
        var def = await db.DataDefinitions.Where(x => x.CaseId == dataCase.Id).SingleAsync(d => d.Name == dataDefinitionName);

        if (!def.IsValid)
            throw new InvalidOperationException($"DataDefinition '{def.Name}' is not valid.");

        var tagsToAdd = new List<Tag>();
        if (tags != null && tags.Count > 0)
            tagsToAdd = await db.Tags.Where(x => x.CaseId == dataCase.Id).Where(t => tags.Contains(t.Name)).ToListAsync();

        foreach (var tag in tagsToAdd.Where(t => t.UniqueDefinition))
        {
            if (await DefinitionAlreadyUsedInTag(db, dataCase.Id, tag.Name, def.Id))
                throw new InvalidOperationException($"The tag '{tag.Name}' is defined as unique and already holds data for the definition '{def.Name}'.");
        }

        var invalidTags = tagsToAdd.Where(tag => tag.AllowedDataDefinitions != null && !(tag.AllowedDataDefinitions.Contains(def.Name) || tag.AllowedSubDataDefinitions != null && tag.AllowedSubDataDefinitions.Contains(def.Name))).ToList();
        if (invalidTags.Any())
            throw new Exception($"DataDefinition '{def.Name}' is not allowed in the following tags: {string.Join(", ", invalidTags.Select(t => t.Name))}");

        var initValue = string.IsNullOrWhiteSpace(def.InitialValue) ? (values ?? new List<string>()) : [def.InitialValue];

        if (def.PathForConnected != null && def.ConnectionType != null)
        {
            if (def.ConnectionType == ConnectionType.Replicated)
            {
                initValue = GetLinkedObject(db, def.PathForConnected, def?.Name ?? null).Values;
            }
            else if (def.ConnectionType == ConnectionType.Fulllink && values != null)
            {
                var linked = GetLinkedObject(db, def.PathForConnected, def?.Name ?? null);
                await UpdateDataEntryAsync(linked.Case.Name, linked.Id, values);
            }
        }

        var entry = new DataEntry
        {
            Case = dataCase,
            DataDefinition = def,
            Tags = tagsToAdd,
            Values = initValue
        };

        if (!entry.IsValid)
            throw new InvalidOperationException("At least one tag or one dataset must be provided");

        db.DataEntries.Add(entry);
        await db.SaveChangesAsync();

        if (def.ValueType == ValueType.UniqueIdentifier)
        {
            entry.Value = new IdGenerator((int)(entry.Id & 0x3FF)).CreateId().ToString();
            await db.SaveChangesAsync();
        }

        if (def.CalculateType == CalculateType.OnInsert && !string.IsNullOrWhiteSpace(def.ActionForCalculated))
            await new ActionExecuteService(_dbContextFactory, this, _dataAnnotationService).ExecuteActionAsync(caseName, def.ActionForCalculated, entry.Id);

        return (entry.Id, ParseValueInRightObject(entry));
    }

    public async Task<string> FindOutIdDataDefinitionNameAsync(string caseName, string topTag, string? idDataDefinitionName = null)
    {
        if (string.IsNullOrWhiteSpace(idDataDefinitionName))
            idDataDefinitionName = (await _dataAnnotationService.GetTags(await _dbContextFactory.CreateDbContextAsync(), caseName)!.SingleOrDefaultAsync(x => x.Name == topTag) ?? throw new InvalidOperationException($"Tag {topTag} couldn't be found")).DefaultIdentifierDefinition;

        return idDataDefinitionName ?? throw new Exception("Can't figure out IdDataDefinitionName");
    }

    public async Task<string> CreateDataEntryWithTagAndIdAsync(string caseName, string topTag, string? idDataDefinitionName = null, List<string>? tags = null)
    {
        if (string.IsNullOrWhiteSpace(idDataDefinitionName))
            idDataDefinitionName = await FindOutIdDataDefinitionNameAsync(caseName, topTag, idDataDefinitionName);

        if ((await GetDataDefinitions(await _dbContextFactory.CreateDbContextAsync(), caseName, idDataDefinitionName)!.SingleAsync()).ValueType != Models.Data.ValueType.UniqueIdentifier)
            throw new InvalidOperationException($"The data definition '{idDataDefinitionName}' is not of type UniqueIdentifier.");

        if (tags == null)
            tags = new List<string>();

        if (!tags.Contains(topTag))
            tags.Add(topTag);

        var result = await CreateDataEntryAsync(caseName, idDataDefinitionName, [], tags);

        if (result.value is not string)
            throw new InvalidOperationException($"The data definition '{idDataDefinitionName}' with id {result.id} cannot have MultipleValues enabled, because it is of type UniqueIdentifier.");

        string newTag = $"{topTag}_{result.value}";
        tags.Add(newTag);

        var topTagEntity = await _dataAnnotationService.GetTags(await _dbContextFactory.CreateDbContextAsync(), caseName)!.SingleOrDefaultAsync(x => x.Name == topTag) ?? throw new InvalidOperationException($"Tag {topTag} couldn't be found");

        await _dataAnnotationService.CreateTagAsync(caseName, new Tag { Name = newTag, Description = newTag, UniqueDefinition = true, DefaultIdentifierDefinition = topTagEntity.DefaultIdentifierDefinition });

        await UpdateDataEntryAsync(caseName, result.id, null, tags, true);

        return newTag;
    }

    public async Task UpdateDataEntryAsync(string caseName, long entryId, List<string>? values = null, List<string>? tags = null, bool skipCalculated = false)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var entry = await db.DataEntries.Include(e => e.DataDefinition).Include(e => e.Tags).Include(e => e.Case).SingleOrDefaultAsync(e => e.Id == entryId && e.Case.Name == caseName) ?? throw new InvalidOperationException($"DataEntry with id {entryId} not found.");

        var def = entry.DataDefinition;

        if (values != null)
            entry.Values = values.ToList();

        if (tags != null)
        {
            var newTags = await db.Tags.Include(x => x.Case).Where(t => t.Case.Name == caseName && tags.Contains(t.Name)).ToListAsync();
            entry.Tags = newTags;
        }

        var invalidTags = entry.Tags.Where(tag => tag.AllowedDataDefinitions != null && !(tag.AllowedDataDefinitions.Contains(def.Name) || tag.AllowedSubDataDefinitions != null && tag.AllowedSubDataDefinitions.Contains(def.Name))).ToList();
        if (invalidTags.Any())
            throw new Exception($"DataDefinition '{def.Name}' is not allowed in the following tags: {string.Join(", ", invalidTags.Select(t => t.Name))}");

        if (def.ReadOnly && values != null)
            throw new Exception("The entry can't be changed as it is readonly.");


        if (!def.IsValid)
            throw new InvalidOperationException($"DataDefinition '{def.Name}' is not valid.");

        if (!entry.IsValid)
            throw new InvalidOperationException("At least one tag or one dataset must be provided");

        if (def.PathForConnected != null && def.ConnectionType == ConnectionType.Fulllink)
        {
            var linked = GetLinkedObject(db, def.PathForConnected, def?.Name ?? null);
            await UpdateDataEntryAsync(linked.Case.Name, linked.Id, values);
        }

        await db.SaveChangesAsync();

        if (def.CalculateType == CalculateType.OnInsert && !string.IsNullOrWhiteSpace(def.ActionForCalculated) && !skipCalculated)
            await new ActionExecuteService(_dbContextFactory, this, _dataAnnotationService).ExecuteActionAsync(entry.Case.Name, def.ActionForCalculated, entry.Id);
    }

    public async Task DeleteDataEntryAsync(string caseName, long entryId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();

        var entry = await db.DataEntries.Include(e => e.Case).SingleAsync(x => x.Case.Name == caseName && x.Id == entryId) ?? throw new InvalidOperationException($"DataEntry with id {entryId} not found.");

        db.DataEntries.Remove(entry);
        await db.SaveChangesAsync();
    }

    private async Task<object> HandleCalculatedAsync(DataEntry entry)
    {
        var def = entry.DataDefinition;

        if (def.CalculateType == CalculateType.OnCall && !string.IsNullOrWhiteSpace(def.ActionForCalculated))
            await new ActionExecuteService(_dbContextFactory, this, _dataAnnotationService).ExecuteActionAsync(entry.Case.Name, def.ActionForCalculated, entry.Id);

        var newEntry = await (await _dbContextFactory.CreateDbContextAsync()).DataEntries.Include(e => e.Case).Include(e => e.DataDefinition).SingleAsync(x => x.Case.Name == entry.Case.Name && x.Id == entry.Id);

        return ParseValueInRightObject(newEntry!);
    }

    private object HandleConnected(ApplicationDbContext db, DataEntry entry)
    {
        if (entry.DataDefinition.PathForConnected == null)
            throw new Exception("Valuetype is connected, but PathForConnected is missing!");

        if (entry.DataDefinition.ConnectionType != null && (entry.DataDefinition.ConnectionType == ConnectionType.Readonly || entry.DataDefinition.ConnectionType == ConnectionType.Fulllink))
        {
            return ParseValueInRightObject(GetLinkedObject(db, entry.DataDefinition.PathForConnected, entry?.DataDefinition?.Name ?? null));
        }
        else if (entry.DataDefinition.ConnectionType != null && entry.DataDefinition.ConnectionType == ConnectionType.Replicated)
        {
            return ParseValueInRightObject(entry);
        }
        
        throw new InvalidOperationException("Either ConnectionType was empty or couldn't be found!");
    }

    private DataEntry GetLinkedObject(ApplicationDbContext db, string pathForConnected, string? dataDefinitionName = null)
    {
        var path = pathForConnected!.Split('.');
        if (path[1] == "{name}")
            path[1] = dataDefinitionName ?? throw new Exception("DataDefinition name couldn't be found!");

        var linked = db.DataEntries.Include(e => e.Case).Include(e => e.DataDefinition).Include(e => e.Tags).Include(e => e.Tags).Where(e => e.Case.Name == path[0] && e.DataDefinition.Name == path[1] && e.Tags.Any(t => t.Name == path[2])).ToList();

        if (linked.Count > 1)
            throw new InvalidOperationException("Too many linked entries were found.");      
        if (!linked.Any())
            throw new InvalidOperationException($"Link '{pathForConnected}' not found.");

        var target = linked.First();
        if (!target.DataDefinition.IsValid)
            throw new InvalidOperationException($"Linked entry '{pathForConnected}' is invalid.");

        return target;
    }

    private object ParseValueInRightObject(DataEntry entry) => entry.DataDefinition.MultipleValues ? entry.Values : entry.Values.FirstOrDefault()!;

    private async Task<bool> DefinitionAlreadyUsedInTag(ApplicationDbContext db, long caseId, string tagName, long dataDefinitionId) => await db.Tags.Where(t => t.CaseId == caseId && t.Name == tagName).AnyAsync(t => t.DataEntries.Any(de => de.DataDefinitionId == dataDefinitionId));
}