using Core.Services.Common;
using Microsoft.AspNetCore.Mvc;

namespace Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaseController(CommonService _commonService) : ControllerBase
{
    [HttpPost("tenant")]
    public async Task<IActionResult> CreateTenantAsync([FromQuery] string name)
    {
        await _commonService.CreateTenantAsync(name);
        return NoContent();
    }

    [HttpPost("tenant/{tenantName}")]
    public async Task<IActionResult> CreateCaseAsync([FromRoute] string tenantName, [FromQuery] string name, [FromQuery] string description)
    {
        await _commonService.CreateCaseAsync(name, description, tenantName);
        return NoContent();
    }
}