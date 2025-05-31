using Core.Db;
using Core.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Data;

public class DataAnnotationService(IDbContextFactory<ApplicationDbContext> _dbContextFactory)
{
    public IQueryable<DataSet>? GetDataSets(ApplicationDbContext db, string caseName)
    {
        return db.DataSets.Include(d => d.DataEntries).ThenInclude(e => e.Tags).Include(d => d.DataEntries).ThenInclude(e => e.DataDefinition).Include(d => d.Case).Where(d => d.Case.Name == caseName);
    }

    public async Task<DataSet> CreateDataSetAsync(string caseName, DataSet dataSet)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var caseEntity = await db.Cases.FirstOrDefaultAsync(c => c.Name == caseName);
        if (caseEntity == null)
            throw new InvalidOperationException($"Case '{caseName}' not found.");

        dataSet.Case = caseEntity;

        await db.DataSets.AddAsync(dataSet);
        await db.SaveChangesAsync();
        return dataSet;
    }

    public async Task<DataSet> UpdateDataSetAsync(string caseName, DataSet updatedDataSet)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var existing = await db.DataSets.Include(d => d.Case).FirstOrDefaultAsync(d => d.Case.Name == caseName && d.Id == updatedDataSet.Id);

        if (existing == null)
            throw new InvalidOperationException($"DataDefinition '{updatedDataSet.Name}' not found in case '{caseName}'.");

        existing.Name = updatedDataSet.Name;
        existing.Description = updatedDataSet.Description;

        db.DataSets.Update(existing);
        await db.SaveChangesAsync();
        return existing;
    }

    public IQueryable<Tag>? GetTags(ApplicationDbContext db, string caseName)
    {
        return db.Tags.Include(d => d.DataEntries).ThenInclude(e => e.DataDefinition).Include(d => d.DataEntries).ThenInclude(e => e.DataSet).Include(d => d.Case).Where(d => d.Case.Name == caseName);
    }

    public async Task<Tag> CreateTagAsync(string caseName, Tag tag)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var caseEntity = await db.Cases.FirstOrDefaultAsync(c => c.Name == caseName);
        if (caseEntity == null)
            throw new InvalidOperationException($"Case '{caseName}' not found.");

        tag.Case = caseEntity;

        await db.Tags.AddAsync(tag);
        await db.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag> UpdateTagAsync(string caseName, Tag updatedTag)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var existing = await db.Tags.Include(d => d.Case).FirstOrDefaultAsync(d => d.Case.Name == caseName && d.Id == updatedTag.Id);

        if (existing == null)
            throw new InvalidOperationException($"DataDefinition '{updatedTag.Name}' not found in case '{caseName}'.");

        existing.Name = updatedTag.Name;
        existing.Description = updatedTag.Description;

        db.Tags.Update(existing);
        await db.SaveChangesAsync();
        return existing;
    }
}
