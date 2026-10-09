using DataService.Domain.IO;
using DataService.Domain.IO.User;
using DataService.Domain.Repository;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;
using DataService.Domain.Services;
using DataService.Domain.Services.Events;
using DataService.Domain.Tools;
using Microsoft.AspNetCore.Identity;

namespace DataService.Infrastructure.Services;

/// <summary>
/// Scoped service
/// </summary>
public class UserService(
    IUserRepository userRepository,
    ITenantRepository tenantRepository,
    IEventPublisher eventPublisher,
    IPasswordHasher<UserLogin> passwordHasher,
    InputValidator validator,
    SwissKnife swissKnife) : IUserService
{
    public async Task<OkOrError<string>> CreateUserAsync(string tenantId, UserCreateDTO command)
    {
        var tenantGuid = validator.SanitizeGuid(tenantId);
        if (!tenantGuid)
            return new OkOrError<string>(false, Error: tenantGuid.Error);

        var username = validator.ValidateUsername(command.Username);
        if (!username)
            return new OkOrError<string>(false, Error: username.Error);

        var password = validator.ValidatePassword(command.Password);
        if (!password)
            return new OkOrError<string>(false, Error: password.Error);

        var groupIds = ParseGuids(command.SecurityGroupIds);
        if (!groupIds)
            return new OkOrError<string>(false, Error: groupIds.Error);

        // the tenant must exist, be active, and have room for a new user
        var tenant = await tenantRepository.GetTenantByTenantIdAsync(tenantId);
        if (!tenant)
            return new OkOrError<string>(false, Error: tenant.Error);
        if (!tenant.Value!.IsActive)
            return new OkOrError<string>(false, Error: $"Tenant {tenantId} is not active.");

        if (tenant.Value.MaxUsers is int maxUsers && command.Enabled)
        {
            var current = await userRepository.CountEnabledUsersAsync(tenantGuid.Value);
            if (!current)
                return new OkOrError<string>(false, Error: current.Error);
            if (current.Value >= maxUsers)
                return new OkOrError<string>(false, Error: $"Tenant {tenantId} reached the maximum number of users ({maxUsers}).");
        }

        // create brand new Ids
        var newUserId = swissKnife.GenerateGuid();
        var newUserLoginId = swissKnife.GenerateGuid();
        var passwordHash = passwordHasher.HashPassword(null!, password.Value!);

        // the repository stores this event in the outbox, in the same transaction as the user
        var createdEvent = new CreatedEvent
        {
            Id = swissKnife.GenerateGuid(),
            Producer = "UserService.CreateUserAsync",
            ResourceName = "User",
            ResourceId = newUserId.ToString(),
            ResourceType = "User"
        };

        var res = await userRepository.CreateUserAsync(
            tenantGuid, newUserId, newUserLoginId, command, groupIds.Value!, passwordHash, createdEvent);

        // nothing was committed: the failure is published on its own
        if (!res.Ok)
            await eventPublisher.PublishEventAsync(new FailureEvent
            {
                Id = swissKnife.GenerateGuid(),
                Producer = "UserService.CreateUserAsync",
                ResourceName = "User",
                ResourceId = newUserId.ToString(),
                ResourceType = "User",
                ErrorMessage = res.Error ?? "Unknown error creating user"
            });

        return res;
    }

    public async Task<OkOrError<UserViewDTO>> GetUserAsync(string tenantId, string userId)
    {
        var ids = ParseTenantAndUser(tenantId, userId);
        if (!ids)
            return new OkOrError<UserViewDTO>(false, Error: ids.Error);

        return await userRepository.GetUserAsync(ids.Value.TenantId, ids.Value.UserId);
    }

    public async Task<OkOrError<QueryResult>> ListUsersAsync(string tenantId, int page, int pageSize)
    {
        var tenantGuid = validator.SanitizeGuid(tenantId);
        if (!tenantGuid)
            return new OkOrError<QueryResult>(false, Error: tenantGuid.Error);

        return await userRepository.ListUsersAsync(tenantGuid.Value, page, pageSize);
    }

    public async Task<OkOrError<UserViewDTO>> UpdateUserAsync(string tenantId, string userId, UserUpdateDTO command)
    {
        var ids = ParseTenantAndUser(tenantId, userId);
        if (!ids)
            return new OkOrError<UserViewDTO>(false, Error: ids.Error);

        string? passwordHash = null;
        if (command.Password != null)
        {
            var password = validator.ValidatePassword(command.Password);
            if (!password)
                return new OkOrError<UserViewDTO>(false, Error: password.Error);
            passwordHash = passwordHasher.HashPassword(null!, password.Value!);
        }

        return await userRepository.UpdateUserAsync(ids.Value.TenantId, ids.Value.UserId, command, passwordHash);
    }

    public async Task<OkOrError<int>> DisableUserAsync(string tenantId, string userId)
    {
        var ids = ParseTenantAndUser(tenantId, userId);
        if (!ids)
            return new OkOrError<int>(false, Error: ids.Error);

        return await userRepository.DisableUserAsync(ids.Value.TenantId, ids.Value.UserId);
    }

    public async Task<OkOrError<int>> SetUserRolesAsync(string tenantId, string userId, UserRolesUpdateDTO command)
    {
        var ids = ParseTenantAndUser(tenantId, userId);
        if (!ids)
            return new OkOrError<int>(false, Error: ids.Error);

        if (command.RoleCodes == null || command.RoleCodes.Any(string.IsNullOrWhiteSpace))
            return new OkOrError<int>(false, Error: "RoleCodes is required and cannot contain empty codes");

        return await userRepository.SetUserRolesAsync(ids.Value.TenantId, ids.Value.UserId, command.RoleCodes);
    }

    public async Task<OkOrError<int>> SetUserSecurityGroupsAsync(string tenantId, string userId, UserSecurityGroupsUpdateDTO command)
    {
        var ids = ParseTenantAndUser(tenantId, userId);
        if (!ids)
            return new OkOrError<int>(false, Error: ids.Error);

        if (command.SecurityGroupIds == null)
            return new OkOrError<int>(false, Error: "SecurityGroupIds is required");

        var groupIds = ParseGuids(command.SecurityGroupIds);
        if (!groupIds)
            return new OkOrError<int>(false, Error: groupIds.Error);

        return await userRepository.SetUserSecurityGroupsAsync(ids.Value.TenantId, ids.Value.UserId, groupIds.Value!);
    }

    private OkOrError<(Guid TenantId, Guid UserId)> ParseTenantAndUser(string tenantId, string userId)
    {
        var tenantGuid = validator.SanitizeGuid(tenantId);
        if (!tenantGuid)
            return new OkOrError<(Guid, Guid)>(false, Error: tenantGuid.Error);

        var userGuid = validator.SanitizeGuid(userId);
        if (!userGuid)
            return new OkOrError<(Guid, Guid)>(false, Error: userGuid.Error);

        return (tenantGuid.Value, userGuid.Value);
    }

    private OkOrError<List<Guid>> ParseGuids(IEnumerable<string>? values)
    {
        var result = new List<Guid>();
        foreach (var value in values ?? [])
        {
            var guid = validator.SanitizeGuid(value);
            if (!guid)
                return new OkOrError<List<Guid>>(false, Error: $"Invalid security group id '{value}': {guid.Error}");
            result.Add(guid.Value);
        }
        return result;
    }
}
