using Core.Db;
using Core.Models.Action;
using Core.Services.Action;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActionsController(ActionService _actionService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [EnableQuery]
    [HttpGet("{caseName}/odata")]
    public async Task<ActionResult<IQueryable<ActionDefinition>>> GetAsync([FromRoute] string caseName)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var actionDefinition = _actionService.GetAllAsync(db, caseName);
        if (actionDefinition == null)
            return NotFound();

        return Ok(actionDefinition);
    }

    [HttpPost("{caseName}")]
    public async Task<ActionResult<ActionDefinition>> CreateAsync([FromRoute] string caseName,
        [FromForm] string name,
        [FromForm] string actionFunction,
        [FromForm] List<string> tagUsedInAction,
        [FromForm] List<string> dataSetUsedInAction)
    {
        await _actionService.CreateAsync(caseName, name, actionFunction, tagUsedInAction, dataSetUsedInAction);
        return NoContent();
    }

    [HttpPost("{caseName}/{actionName}/execute")]
    public async Task<IActionResult> ExecuteActionAsync([FromRoute] string caseName, [FromRoute] string actionName)
    {
        await _actionService.ExecuteActionAsync(caseName, actionName, null);
        return NoContent();
    }

    [HttpPut("{caseName}/{actionName}")]
    public async Task<ActionResult<ActionDefinition>> UpdateAsync([FromRoute] string caseName, [FromRoute] string actionName,
        [FromForm] string actionFunction,
        [FromForm] List<string> tagUsedInAction,
        [FromForm] List<string> dataSetUsedInAction,
        [FromForm] string? newName = null)
    {
        await _actionService.UpdateAsync(caseName, actionName, actionFunction, tagUsedInAction, dataSetUsedInAction, newName);
        return NoContent();
    }

    [HttpDelete("{caseName}/{actionName}")]
    public async Task<ActionResult<ActionDefinition>> DeleteAsync([FromRoute] string caseName, [FromRoute] string actionName)
    {
        await _actionService.DeleteAsync(caseName, actionName);
        return NoContent();
    }
}