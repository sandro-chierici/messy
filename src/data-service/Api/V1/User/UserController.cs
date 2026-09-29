using DataService.Domain.IO;
using DataService.Domain.IO.User;
using DataService.Domain.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataService.Api.V1.User;

[ApiController]
[Route(BaseUri)]
public class UserController(
    IUserService userService
    ) : ApiControllerBase
{
    private const string BaseUri = "/api/v1/tenant/{tenantId}/users";
    private const int MaxPageSize = 200;

    [HttpGet]
    public async Task<IActionResult> ListUsers(string tenantId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (page < 1 || pageSize < 1 || pageSize > MaxPageSize)
            return BadRequest(ApiV1Response.Failure($"page must be >= 1 and pageSize between 1 and {MaxPageSize}"));

        var res = await userService.ListUsersAsync(tenantId, page, pageSize);
        if (!res)
            return Failure(res, "Failed to list users");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUser(string tenantId, string userId)
    {
        var res = await userService.GetUserAsync(tenantId, userId);
        if (!res)
            return Failure(res, "User not found");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(string tenantId, [FromBody] UserCreateDTO userCommand)
    {
        var res = await userService.CreateUserAsync(tenantId, userCommand);
        if (!res)
            return Failure(res, "Failed to create user");

        return Created(
            uri: $"/api/v1/tenant/{tenantId}/users/{res.Value}",
            value: ApiV1Response.Created(new { UserId = res.Value }));
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUser(string tenantId, string userId, [FromBody] UserUpdateDTO userCommand)
    {
        var res = await userService.UpdateUserAsync(tenantId, userId, userCommand);
        if (!res)
            return Failure(res, "Failed to update user");

        return Ok(ApiV1Response.Read(data: res.Value!));
    }

    /// <summary>
    /// Soft delete: the login is disabled and the party deactivated; nothing is removed.
    /// </summary>
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DisableUser(string tenantId, string userId)
    {
        var res = await userService.DisableUserAsync(tenantId, userId);
        if (!res)
            return Failure(res, "Failed to disable user");

        return NoContent();
    }

    [HttpPut("{userId}/roles")]
    public async Task<IActionResult> SetUserRoles(string tenantId, string userId, [FromBody] UserRolesUpdateDTO command)
    {
        var res = await userService.SetUserRolesAsync(tenantId, userId, command);
        if (!res)
            return Failure(res, "Failed to set user roles");

        return Ok(ApiV1Response.Read(new { RoleCount = res.Value }));
    }

    [HttpPut("{userId}/security-groups")]
    public async Task<IActionResult> SetUserSecurityGroups(string tenantId, string userId, [FromBody] UserSecurityGroupsUpdateDTO command)
    {
        var res = await userService.SetUserSecurityGroupsAsync(tenantId, userId, command);
        if (!res)
            return Failure(res, "Failed to set user security groups");

        return Ok(ApiV1Response.Read(new { SecurityGroupCount = res.Value }));
    }
}
