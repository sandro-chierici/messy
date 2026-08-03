using DataService.Business.IO;
using DataService.Business.Rules;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Tenant;

[Route("api/v1/tenant")]
[ApiController]
public class TenantController(
    InputValidator validator
    ) : ControllerBase
{
    private readonly InputValidator _inputValidator = validator;

    [HttpGet]
    [Route("{tenantId}")]
    public IActionResult GetTenant(string tenantId)
    {
        var id = _inputValidator.SanitizeId(tenantId);

        if (!id)
            return BadRequest(ApiV1Response.Failure(id.Error ?? "Invalid tenantId"));

        // Implement your logic to retrieve tenant information here
        QueryResult res = QueryResult.Create(
            totalCount: 1, 
            new
            {
                Id = id,
                Name = "Sample Tenant",
                Description = "This is a sample tenant description.",
                CreatedDate = DateTime.UtcNow.AddMonths(-1),
                UpdatedDate = DateTime.UtcNow
            });

        return Ok(ApiV1Response.Read(res));
    }


}
