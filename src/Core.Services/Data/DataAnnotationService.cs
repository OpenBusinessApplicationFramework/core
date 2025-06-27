using Core.Db;
using Core.Models.Data;
using Microsoft.EntityFrameworkCore;

namespace Core.Services.Data;

public class DataAnnotationService(IDbContextFactory<ApplicationDbContext> _dbContextFactory)
{
    public IQueryable<Tag>? GetTags(ApplicationDbContext db, string caseName, string? getSubTagsFromTopTag = null)
    {
        var query = db.Tags.Include(d => d.DataEntries).ThenInclude(e => e.DataDefinition).Include(d => d.DataEntries).Include(d => d.Case).Where(d => d.Case.Name == caseName);

        if (!string.IsNullOrWhiteSpace(getSubTagsFromTopTag))
            query = query.Where(e => e.Name.StartsWith($"{getSubTagsFromTopTag}_"));

        return query;
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

    public async Task<Tag> UpdateTagAsync(string caseName, string name, string? description = null, bool? uniqueDefinition = null, string? newName = null)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        var existing = await db.Tags.Include(d => d.Case).FirstOrDefaultAsync(d => d.Case.Name == caseName && d.Name == name);

        if (existing == null)
            throw new InvalidOperationException($"Tag '{existing.Name}' not found in case '{caseName}'.");

        if (description != null)
            existing.Description = description;

        if (uniqueDefinition != null)
            existing.UniqueDefinition = (bool)uniqueDefinition;

        if (newName != null)
            existing.Name = newName;

        db.Tags.Update(existing);
        await db.SaveChangesAsync();
        return existing;
    }
}
