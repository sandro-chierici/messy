using DataService.Business.IO;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Tenant;

[Route("api/v1/tenant")]
[ApiController]
public class TenantController : ControllerBase
{
    public TenantController() { }

    [HttpGet]
    [Route("{tenantId}")]
    public IActionResult GetTenant(string tenantId)
    {
        if (tenantId == null)
            return BadRequest(ApiV1Response.Failure("Invalid tenantId"));
        // Implement your logic to retrieve tenant information here
        QueryResponse queryResponse = QueryResponse.Create(
            totalCount: 1, 
            new
            {
                Id = tenantId,
                Name = "Sample Tenant",
                Description = "This is a sample tenant description.",
                CreatedDate = DateTime.UtcNow.AddMonths(-1),
                UpdatedDate = DateTime.UtcNow
            });

        return Ok(ApiV1Response.Read(queryResponse));
    }
}
