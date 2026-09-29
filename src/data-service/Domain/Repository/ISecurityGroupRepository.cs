using DataService.Domain.IO.Security;
using DataService.Domain.Rules;

namespace DataService.Domain.Repository;

public interface ISecurityGroupRepository : IRepository
{
    public Task<OkOrError<string>> CreateSecurityGroupAsync(Guid tenantId, Guid securityGroupId, SecurityGroupCreateDTO command);
    public Task<OkOrError<SecurityGroupViewDTO>> GetSecurityGroupAsync(Guid tenantId, Guid securityGroupId);
    public Task<OkOrError<List<SecurityGroupViewDTO>>> ListSecurityGroupsAsync(Guid tenantId);
}
