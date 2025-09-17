using Core.Db;
using Core.Models.Action;
using Core.Services.Action;
using Core.Services.Data;
using Core.Utils.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActionsController(ActionService _actionService, DataService _dataService, DataAnnotationService _dataAnnotationService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [HttpGet("{caseName}/odata")]
    public async Task<ActionResult<IQueryable<ActionDefinition>>> GetAsync(ODataQueryOptions<ActionDefinition> queryOptions, [FromRoute] string caseName)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var actionDefinition = _actionService.GetAllAsync(db, caseName);
        if (actionDefinition == null)
            return NotFound();

        return Ok(queryOptions.ApplyTo(actionDefinition));
    }

    [HttpPost("{caseName}")]
    public async Task<ActionResult<ActionDefinition>> CreateAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string actionFunction,
        [FromForm] List<string> tagUsedInAction,
        [FromForm] List<string> tagViaArgument,
        [FromForm] List<string> valueViaArgument)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _actionService.CreateAsync(caseName, name, actionFunction, tagUsedInAction, tagViaArgument, valueViaArgument);
        });

        return NoContent();
    }

    [HttpPost("{caseName}/{actionName}/execute")]
    public async Task<IActionResult> ExecuteActionAsync([FromRoute] string caseName, [FromRoute] string actionName, [FromForm] List<string>? tagArguments, [FromForm] Dictionary<string,string>? arguments, string? callingSubTag = null)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await new ActionExecuteService(_dbContextFactory, _dataService, _dataAnnotationService).ExecuteActionAsync(caseName, actionName, null, tagArguments, arguments, callingSubTag);
        });

        return NoContent();
    }

    [HttpPut("{caseName}/{actionName}")]
    public async Task<ActionResult<ActionDefinition>> UpdateAsync([FromRoute] string caseName, [FromRoute] string actionName,
        [FromForm] string actionFunction,
        [FromForm] List<string> tagUsedInAction,
        [FromForm] List<string> tagViaArgument,
        [FromForm] List<string> valueViaArgument,
        [FromForm] string? newName = null)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _actionService.UpdateAsync(caseName, actionName, actionFunction, tagUsedInAction, tagViaArgument, valueViaArgument, newName);
        });
        
        return NoContent();
    }

    [HttpDelete("{caseName}/{actionName}")]
    public async Task<ActionResult<ActionDefinition>> DeleteAsync([FromRoute] string caseName, [FromRoute] string actionName)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _actionService.DeleteAsync(caseName, actionName);
        });
        
        return NoContent();
    }
}