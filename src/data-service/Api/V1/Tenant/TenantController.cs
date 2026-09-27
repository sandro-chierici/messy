using DataService.Domain.IO;
using DataService.Domain.IO.Tenant;
using DataService.Domain.Rules;
using DataService.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Tenant;

[ApiController]
[Route(BaseUri)]
public class TenantController(
    InputValidator validator,
    ITenantService tenantService
    ) : ControllerBase
{
    private const string BaseUri = "/api/v1/tenant";

    [HttpGet]
    [Route("{tenantId}")]
    public async Task<IActionResult> GetTenant(string tenantId)
    {
        var safeId = validator.SanitizeId(tenantId);

        if (!safeId)
            return BadRequest(ApiV1Response.Failure(safeId.Error ?? "Invalid tenantId"));

        var res = await tenantService.GetTenantAsync(safeId!);
        if (!res)
            return NotFound(ApiV1Response.Failure(res.Error ?? "TenantId not found"));

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant([FromBody] TenantCreateDTO tenantCommand)
    {
        var res = await tenantService.CreateTenantAsync(tenantCommand);
        if (!res)
            return BadRequest(ApiV1Response.Failure(res.Error ?? "Failed to create tenant"));
        
        return Created(
            uri: $"{BaseUri}/{res.Value}", 
            value: ApiV1Response.Read(new { TenantId = res.Value }));
    }

}
