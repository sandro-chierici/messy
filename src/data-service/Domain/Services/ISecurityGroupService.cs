using DataService.Domain.IO.Security;
using DataService.Domain.Rules;

namespace DataService.Domain.Services;

public interface ISecurityGroupService : IBackendService
{
    Task<OkOrError<string>> CreateSecurityGroupAsync(string tenantId, SecurityGroupCreateDTO command);
    Task<OkOrError<SecurityGroupViewDTO>> GetSecurityGroupAsync(string tenantId, string securityGroupId);
    Task<OkOrError<List<SecurityGroupViewDTO>>> ListSecurityGroupsAsync(string tenantId);
}
