using Core.Services.Common;
using Core.Utils.Transactions;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaseController(CommonService _commonService) : ControllerBase
{
    [HttpPost("{tenantName}")]
    public async Task<IActionResult> CreateCaseAsync([FromRoute] string tenantName, [FromQuery] string name, [FromQuery] string description)
    {
        await TransactionScopeHelper.ExecuteInTransactionAsync(new TransactionScopeHelperSettings(), async () =>
        {
            await _commonService.CreateCaseAsync(name, description, tenantName);
        });
        
        return NoContent();
    }
}