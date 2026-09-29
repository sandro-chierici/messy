using DataService.Domain.IO.Reference;
using DataService.Domain.IO.Security;
using DataService.Domain.Repository.Models;

namespace DataService.Domain.Mapper.Security;

public class SecurityGroupMapper
{
    public SecurityGroupViewDTO MapSecurityGroupViewFrom(SecurityGroup group, IEnumerable<string> permissionCodes) =>
        new SecurityGroupViewDTO
        {
            SecurityGroupId = $"{group.SecurityGroupId}",
            TenantId = group.TenantId?.ToString(),
            IsBuiltIn = group.TenantId == null,
            Name = group.Name,
            Description = group.Description,
            PermissionCodes = permissionCodes.ToList()
        };

    public RoleTypeViewDTO MapRoleTypeViewFrom(RoleType roleType) =>
        new RoleTypeViewDTO
        {
            Code = roleType.Code,
            ParentCode = roleType.ParentCode,
            Description = roleType.Description
        };

    public SecurityPermissionViewDTO MapSecurityPermissionViewFrom(SecurityPermission permission) =>
        new SecurityPermissionViewDTO
        {
            Code = permission.Code,
            Description = permission.Description
        };
}
