using DataService.Business.IO;
using DataService.Business.IO.DataView;
using DataService.Business.Repository;
using DataService.Business.Rules;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Tenant;

[Route("api/v1/tenant")]
[ApiController]
public class TenantController(
    InputValidator validator,
    ITenantRepository tenantRepository
    ) : ControllerBase
{
    private readonly InputValidator _inputValidator = validator;
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    [HttpGet]
    [Route("{tenantId}")]
    public async Task<IActionResult> GetTenant(string tenantId)
    {
        var safeId = _inputValidator.SanitizeId(tenantId);

        if (!safeId)
            return BadRequest(ApiV1Response.Failure(safeId.Error ?? "Invalid tenantId"));

        var res = await _tenantRepository.GetTenantByTenantIdAsync(safeId!);
        if (!res)
            return NotFound(ApiV1Response.Failure(res.Error ?? "TenantId not found"));

        return Ok(ApiV1Response.Read(res));
    }

}
