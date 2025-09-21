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
public class DataDefinitionsController(DataService _dataService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [HttpGet("{caseName}/odata")]
    public async Task<ActionResult<IQueryable<DataDefinition>>> GetAsync(ODataQueryOptions<DataDefinition> queryOptions, [FromRoute] string caseName)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var definition = _dataService.GetDataDefinitions(db, caseName);
        if (definition == null)
            return NotFound();

        return Ok(queryOptions.ApplyTo(definition));
    }

    [HttpPost("{caseName}")]
    public async Task<ActionResult<DataDefinition>> CreateAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] bool multipleValues,
        [FromForm] bool readOnly = false,
        [FromForm] string? initialValue = null,
        [FromForm] Models.Data.ValueType valueType = Models.Data.ValueType.Static,
        [FromForm] string? actionForCalculated = null,
        [FromForm] ConnectionType? connectionType = null,
        [FromForm] string? pathForConnected = null)
    {
        var definition = new DataDefinition
        {
            Name = name,
            MultipleValues = multipleValues,
            ReadOnly = readOnly,
            InitialValue = initialValue,
            ValueType = valueType,
            ActionForCalculated = actionForCalculated,
            ConnectionType = connectionType,
            PathForConnected = pathForConnected
        };

        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataService.CreateDataDefinitionAsync(caseName, definition);
        });

        return NoContent();
    }

    [HttpPut("{caseName}/{definitionId:long}")]
    public async Task<ActionResult<DataDefinition>> UpdateAsync([FromRoute] string caseName, [FromRoute] long definitionId,
        [FromForm] string name,
        [FromForm] bool multipleValues,
        [FromForm] bool readOnly = false,
        [FromForm] string? initialValue = null,
        [FromForm] Models.Data.ValueType valueType = Models.Data.ValueType.Static,
        [FromForm] string? actionForCalculated = null,
        [FromForm] ConnectionType? connectionType = null,
        [FromForm] string? pathForConnected = null)
    {
        var definition = new DataDefinition
        {
            Id = definitionId,
            Name = name,
            MultipleValues = multipleValues,
            ReadOnly = readOnly,
            InitialValue = initialValue,
            ValueType = valueType,
            ActionForCalculated = actionForCalculated,
            ConnectionType = connectionType,
            PathForConnected = pathForConnected
        };

        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataService.UpdateDataDefinitionAsync(caseName, definition);
        });

        return NoContent();
    }

    [HttpDelete("{caseName}/{definitionId:long}")]
    public async Task<IActionResult> DeleteAsync([FromRoute] string caseName, [FromRoute] long definitionId)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _dataService.DeleteDataDefinitionAsync(caseName, definitionId);
        });
        
        return NoContent();
    }
}