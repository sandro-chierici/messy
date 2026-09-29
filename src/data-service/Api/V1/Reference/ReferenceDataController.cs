using DataService.Domain.IO;
using DataService.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Reference;

[ApiController]
[Route("/api/v1")]
public class ReferenceDataController(
    IReferenceDataService referenceDataService
    ) : ApiControllerBase
{
    [HttpGet("role-types")]
    public async Task<IActionResult> GetRoleTypes()
    {
        var res = await referenceDataService.GetRoleTypesAsync();
        if (!res)
            return Failure(res, "Failed to read role types");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    [HttpGet("security-permissions")]
    public async Task<IActionResult> GetSecurityPermissions()
    {
        var res = await referenceDataService.GetSecurityPermissionsAsync();
        if (!res)
            return Failure(res, "Failed to read security permissions");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }
}
