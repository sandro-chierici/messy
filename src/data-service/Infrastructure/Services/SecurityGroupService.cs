using DataService.Domain.IO.Security;
using DataService.Domain.Repository;
using DataService.Domain.Rules;
using DataService.Domain.Services;
using DataService.Domain.Services.Events;
using DataService.Domain.Tools;

namespace DataService.Infrastructure.Services;

/// <summary>
/// Scoped service
/// </summary>
public class SecurityGroupService(
    ISecurityGroupRepository securityGroupRepository,
    IEventPublisher eventPublisher,
    InputValidator validator,
    SwissKnife swissKnife) : ISecurityGroupService
{
    private const int MaxNameLength = 100;

    public async Task<OkOrError<string>> CreateSecurityGroupAsync(string tenantId, SecurityGroupCreateDTO command)
    {
        var tenantGuid = validator.SanitizeGuid(tenantId);
        if (!tenantGuid)
            return new OkOrError<string>(false, Error: tenantGuid.Error);

        if (string.IsNullOrWhiteSpace(command.Name) || command.Name.Length > MaxNameLength)
            return new OkOrError<string>(false, Error: $"Name is required, max {MaxNameLength} chars");

        // create a brand new Id
        var newSecurityGroupId = swissKnife.GenerateGuid();

        var res = await securityGroupRepository.CreateSecurityGroupAsync(tenantGuid.Value, newSecurityGroupId, command);

        EventBase ev = res.Ok
            ? new CreatedEvent
            {
                Id = swissKnife.GenerateGuid(),
                Producer = "SecurityGroupService.CreateSecurityGroupAsync",
                ResourceName = "SecurityGroup",
                ResourceId = newSecurityGroupId.ToString(),
                ResourceType = "SecurityGroup"
            }
            : new FailureEvent
            {
                Id = swissKnife.GenerateGuid(),
                Producer = "SecurityGroupService.CreateSecurityGroupAsync",
                ResourceName = "SecurityGroup",
                ResourceId = newSecurityGroupId.ToString(),
                ResourceType = "SecurityGroup",
                ErrorMessage = res.Error ?? "Unknown error creating security group"
            };

        await eventPublisher.PublishEventAsync(ev);

        return res;
    }

    public async Task<OkOrError<SecurityGroupViewDTO>> GetSecurityGroupAsync(string tenantId, string securityGroupId)
    {
        var tenantGuid = validator.SanitizeGuid(tenantId);
        if (!tenantGuid)
            return new OkOrError<SecurityGroupViewDTO>(false, Error: tenantGuid.Error);

        var groupGuid = validator.SanitizeGuid(securityGroupId);
        if (!groupGuid)
            return new OkOrError<SecurityGroupViewDTO>(false, Error: groupGuid.Error);

        return await securityGroupRepository.GetSecurityGroupAsync(tenantGuid.Value, groupGuid.Value);
    }

    public async Task<OkOrError<List<SecurityGroupViewDTO>>> ListSecurityGroupsAsync(string tenantId)
    {
        var tenantGuid = validator.SanitizeGuid(tenantId);
        if (!tenantGuid)
            return new OkOrError<List<SecurityGroupViewDTO>>(false, Error: tenantGuid.Error);

        return await securityGroupRepository.ListSecurityGroupsAsync(tenantGuid.Value);
    }
}
