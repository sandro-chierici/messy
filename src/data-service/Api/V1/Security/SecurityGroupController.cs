using DataService.Domain.IO;
using DataService.Domain.IO.Security;
using DataService.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.Security;

[ApiController]
[Route(BaseUri)]
public class SecurityGroupController(
    ISecurityGroupService securityGroupService
    ) : ApiControllerBase
{
    private const string BaseUri = "/api/v1/tenant/{tenantId}/security-groups";

    [HttpGet]
    public async Task<IActionResult> ListSecurityGroups(string tenantId)
    {
        var res = await securityGroupService.ListSecurityGroupsAsync(tenantId);
        if (!res)
            return Failure(res, "Failed to list security groups");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    [HttpGet("{securityGroupId}")]
    public async Task<IActionResult> GetSecurityGroup(string tenantId, string securityGroupId)
    {
        var res = await securityGroupService.GetSecurityGroupAsync(tenantId, securityGroupId);
        if (!res)
            return Failure(res, "Security group not found");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> CreateSecurityGroup(string tenantId, [FromBody] SecurityGroupCreateDTO command)
    {
        var res = await securityGroupService.CreateSecurityGroupAsync(tenantId, command);
        if (!res)
            return Failure(res, "Failed to create security group");

        return Created(
            uri: $"/api/v1/tenant/{tenantId}/security-groups/{res.Value}",
            value: ApiV1Response.Created(new { SecurityGroupId = res.Value }));
    }
}
