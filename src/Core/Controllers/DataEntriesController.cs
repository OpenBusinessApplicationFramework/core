using Core.Db;
using Core.Services.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataEntriesController(DataService _dataService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [HttpGet("{caseName}/{definitionName}")]
    public async Task<ActionResult<IEnumerable<object>>> GetEntriesAsync([FromRoute] string caseName, [FromRoute] string definitionName, [FromQuery] string[]? tags = null, [FromQuery] string[]? dataSetNames = null)
    {
        var entries = await _dataService.GetDataEntriesAsync(caseName, definitionName, tags, dataSetNames);

        return Ok(entries.Select(x => new { x.Id, x.CaseId, x.DataDefinitionId, x.DataSetId, DataDefinitionName = x.DataDefinition.Name, DataSetName = x?.DataSet?.Name ?? null, TagsNames = x.Tags.Any() ? string.Join(", ", x.Tags.Select(x => x.Name)) : null, x.DataDefinition.MultipleValues, x.Value, x.Values, x.IsValid }));
    }

    [HttpPost("{caseName}/{definitionName}")]
    public async Task<ActionResult<object>> CreateEntryAsync([FromRoute] string caseName, [FromRoute] string definitionName, [FromBody] CreateEntryDto dto)
    {
        var created = await _dataService.CreateDataEntryAsync(caseName, definitionName, dto.Values, dto.Tags, dto.DataSetName);

        return NoContent();
    }

    [HttpPut("{entryId:long}")]
    public async Task<ActionResult<object>> UpdateEntryAsync([FromRoute] long entryId, [FromBody] UpdateEntryDto dto)
    {
        var updated = await _dataService.UpdateDataEntryAsync(entryId, dto.Values, dto.Tags, dto.DataSetName);

        return NoContent();
    }

    [HttpDelete("{entryId:long}")]
    public async Task<IActionResult> DeleteEntryAsync([FromRoute] long entryId)
    {
        await _dataService.DeleteDataEntryAsync(entryId);
        return NoContent();
    }

    // DTO-Klassen für Einträge
    public class CreateEntryDto
    {
        public List<string> Values { get; set; } = new();
        public List<string>? Tags { get; set; }
        public string? DataSetName { get; set; }
    }

    public class UpdateEntryDto
    {
        public List<string> Values { get; set; } = new();
        public List<string>? Tags { get; set; }
        public string? DataSetName { get; set; }
    }
}