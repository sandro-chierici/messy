using DataService.Business.IO;
using DataService.Business.IO.Tenant;
using DataService.Business.Rules;
using DataService.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Tenant;

[Route("api/v1/tenant")]
[ApiController]
public class TenantController(
    InputValidator validator,
    ITenantService tenantService
    ) : ControllerBase
{
    private readonly InputValidator _inputValidator = validator;

    [HttpGet]
    [Route("{tenantId}")]
    public async Task<IActionResult> GetTenant(string tenantId)
    {
        var safeId = _inputValidator.SanitizeId(tenantId);

        if (!safeId)
            return BadRequest(ApiV1Response.Failure(safeId.Error ?? "Invalid tenantId"));

        var res = await tenantService.GetTenantAsync(safeId!);
        if (!res)
            return NotFound(ApiV1Response.Failure(res.Error ?? "TenantId not found"));

        return Ok(ApiV1Response.Read(res));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] TenantCommand tenantCommand)
    {
        var res = await tenantService.CreateTenantAsync(tenantCommand);
        if (!res)
            return BadRequest(ApiV1Response.Failure(res.Error ?? "Failed to create tenant"));

        return Ok(ApiV1Response.Read(res));
    }

}
