﻿using Core.Db;
using Core.Models.Data;
using Core.Services.Data;
using Core.Utils.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataAnnotationController(DataAnnotationService _dataAnnotationService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [HttpGet("{caseName}/tag/odata")]
    public async Task<ActionResult<IQueryable<Tag>>> GetTagsAsync(ODataQueryOptions<Tag> queryOptions, [FromRoute] string caseName, [FromQuery] string? getSubTagsFromTopTag = null)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var query = _dataAnnotationService.GetTags(db, caseName, getSubTagsFromTopTag);
        if (query == null)
            return NotFound();

        return Ok(queryOptions.ApplyTo(query));
    }

    [HttpPost("{caseName}/tag")]
    public async Task<ActionResult> CreateTagAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string description,
        [FromForm] bool uniqueDefinition)
    {
        var tag = new Tag
        {
            Name = name,
            Description = description,
            UniqueDefinition = uniqueDefinition
        };

        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataAnnotationService.CreateTagAsync(caseName, tag);
        });
        
        return NoContent();
    }

    [HttpPut("{caseName}/tag")]
    public async Task<ActionResult> UpdateTagAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string? description = null,
        [FromForm] bool? uniqueDefinition = null,
        [FromForm] string? newName = null)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataAnnotationService.UpdateTagAsync(caseName, name, description, uniqueDefinition, newName);
        });

        return NoContent();
    }
}
