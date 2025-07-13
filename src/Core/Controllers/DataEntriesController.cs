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
public class DataEntriesController(DataService _dataService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [HttpGet("{caseName}/inmemoryodata")]
    public async Task<ActionResult<IQueryable<DataEntry>>> GetEntriesAsync(ODataQueryOptions<DataEntry> queryOptions, [FromRoute] string caseName, [FromQuery] string? definitionName = null, [FromQuery] string[]? tags = null, [FromQuery] string? getSubTagsFromTopTag = null, [FromQuery] string? globalFilter = null)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var entries = await _dataService.GetDataEntriesAsync(db, caseName, definitionName, tags, getSubTagsFromTopTag, globalFilter);

        return Ok(queryOptions.ApplyTo(entries.AsQueryable()));
    }

    [HttpPost("{caseName}/single/{definitionName}")]
    public async Task<ActionResult<object>> CreateEntryAsync([FromRoute] string caseName, [FromRoute] string definitionName, [FromBody] CreateEntryDto dto)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataService.CreateDataEntryAsync(caseName, definitionName, dto.Values, dto.Tags);
        });

        return NoContent();
    }

    [HttpPost("{caseName}/multiple")]
    public async Task<ActionResult<object>> CreateMultipleEntriesAsync([FromRoute] string caseName, [FromBody] List<CreateMultipleEntriesDto> dtos)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            foreach (var dto in dtos)
            {
                await _dataService.CreateDataEntryAsync(caseName, dto.DefinitionName, dto.Values, dto.Tags);
            }
        });

        return NoContent();
    }

    [HttpPost("{caseName}/multiple/{topTag}")]
    public async Task<ActionResult<object>> CreateMultipleEntriesWithTagAndIdAsync([FromRoute] string caseName, [FromRoute] string topTag, [FromQuery] string idDataDefinitionName, [FromBody] List<CreateMultipleEntriesDto> dtos)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            var newTag = await _dataService.CreateDataEntryWithTagAndIdAsync(caseName, topTag, idDataDefinitionName, dtos.SingleOrDefault(x => x.DefinitionName == idDataDefinitionName)?.Tags);

            foreach (var dto in dtos.Where(x => x.DefinitionName != idDataDefinitionName))
            {
                if (dto.Tags == null)
                    dto.Tags = new List<string>();

                if (!dto.Tags.Contains(topTag))
                    dto.Tags.Add(topTag);

                dto.Tags.Add(newTag);
                await _dataService.CreateDataEntryAsync(caseName, dto.DefinitionName, dto.Values, dto.Tags);
            }
        });

        return NoContent();
    }

    [HttpPut("{caseName}/{entryId:long}")]
    public async Task<ActionResult<object>> UpdateEntryAsync([FromRoute] string caseName, [FromRoute] long entryId, [FromBody] UpdateEntryDto dto)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataService.UpdateDataEntryAsync(caseName, entryId, dto.Values, dto.Tags);
        });

        return NoContent();
    }

    [HttpDelete("{entryId:long}")]
    public async Task<IActionResult> DeleteEntryAsync([FromRoute] long entryId)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataService.DeleteDataEntryAsync(entryId);
        });

        return NoContent();
    }

    // DTO classes for entries

    public class CreateMultipleEntriesDto : CreateEntryDto
    {
        public string DefinitionName { get; set; }
    }

    public class CreateEntryDto
    {
        public List<string> Values { get; set; } = new();
        public List<string>? Tags { get; set; }
    }

    public class UpdateEntryDto
    {
        public List<string> Values { get; set; } = new();
        public List<string>? Tags { get; set; }
    }
}