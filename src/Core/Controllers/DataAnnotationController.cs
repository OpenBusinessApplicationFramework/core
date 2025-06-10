using Core.Db;
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
    [HttpGet("{caseName}/dataset/odata")]
    public async Task<ActionResult<IQueryable<DataSet>>> GetDataSetsAsync(ODataQueryOptions<DataSet> queryOptions, [FromRoute] string caseName)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var query = _dataAnnotationService.GetDataSets(db, caseName);
        if (query == null)
            return NotFound();

        return Ok(queryOptions.ApplyTo(query));
    }

    [HttpPost("{caseName}/dataset")]
    public async Task<ActionResult<DataSet>> CreateDataSetAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string description)
    {
        var dataSet = new DataSet
        {
            Name = name,
            Description = description
        };

        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataAnnotationService.CreateDataSetAsync(caseName, dataSet);
        });
        
        return NoContent();
    }

    [HttpPut("{caseName}/dataset")]
    public async Task<ActionResult<DataSet>> UpdateDataSetAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string description)
    {
        var dataSet = new DataSet
        {
            Name = name,
            Description = description
        };

        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataAnnotationService.UpdateDataSetAsync(caseName, dataSet);
        });

        return NoContent();
    }

    [HttpGet("{caseName}/tag/odata")]
    public async Task<ActionResult<IQueryable<Tag>>> GetTagsAsync(ODataQueryOptions<Tag> queryOptions, [FromRoute] string caseName)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var query = _dataAnnotationService.GetTags(db, caseName);
        if (query == null)
            return NotFound();

        return Ok(queryOptions.ApplyTo(query));
    }

    [HttpPost("{caseName}/tag")]
    public async Task<ActionResult> CreateTagAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string description)
    {
        var tag = new Tag
        {
            Name = name,
            Description = description
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
        [FromForm] string description)
    {
        var tag = new Tag
        {
            Name = name,
            Description = description
        };

        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataAnnotationService.UpdateTagAsync(caseName, tag);
        });

        return NoContent();
    }
}
