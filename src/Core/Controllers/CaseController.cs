using Core.Db;
using Core.Models.Action;
using Core.Models.Common;
using Core.Services.Action;
using Core.Services.Common;
using Core.Utils.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaseController(CommonService _commonService, IDbContextFactory<ApplicationDbContext> _dbContextFactory) : ControllerBase
{
    [HttpGet("odata")]
    public async Task<IActionResult> GetCaseAsync(ODataQueryOptions<ActionDefinition> queryOptions, [FromQuery] string name, [FromQuery] string description)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        var cases = _commonService.GetCasesAsync(db);
        if (cases == null)
            return NotFound();

        return Ok(queryOptions.ApplyTo(cases));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCaseAsync([FromQuery] string name, [FromForm] string description)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.CreateCaseAsync(name, description);
        });
        
        return NoContent();
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCaseAsync([FromQuery] string name, [FromForm] string? description = null, [FromForm] string? newName = null)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.UpdateCaseAsync(name, description, newName);
        });

        return NoContent();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteCaseAsync([FromQuery] string name)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.DeleteCaseAsync(name);
        });

        return NoContent();
    }

    [HttpGet("{caseName}/mainmenu")]
    public async Task<Dictionary<string, List<MainMenuItem>>> GetMainMenuItemAsync([FromRoute] string caseName)
    {
        var db = await _dbContextFactory.CreateDbContextAsync();
        return await _commonService.GetMainMenuItem(db, caseName);
    }

    [HttpPost("{caseName}/mainmenu")]
    public async Task<IActionResult> CreateMainMenuItemAsync([FromRoute] string caseName, [FromForm] string name, [FromForm] string category, [FromForm] string topTag)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.CreateMainMenuItem(caseName, name, category, topTag);
        });

        return NoContent();
    }

    [HttpPut("{caseName}/mainmenu")]
    public async Task<IActionResult> UpdateMainMenuItemAsync([FromRoute] string caseName, [FromForm] string name, [FromForm] string? category = null, [FromForm] string? topTag = null, [FromForm] string? newName = null)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.UpdateMainMenuItem(caseName, name, category, topTag, newName);
        });

        return NoContent();
    }

    [HttpDelete("{caseName}/mainmenu")]
    public async Task<IActionResult> DeleteMainMenuItemAsync([FromRoute] string caseName, [FromQuery] string name)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.DeleteMainMenuItem(caseName, name);
        });

        return NoContent();
    }
}