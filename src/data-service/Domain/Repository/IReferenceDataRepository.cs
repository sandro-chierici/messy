using DataService.Domain.IO.Reference;
using DataService.Domain.Rules;

namespace DataService.Domain.Repository;

/// <summary>
/// Read-only reference tables (RoleType, SecurityPermission).
/// </summary>
public interface IReferenceDataRepository : IRepository
{
    public Task<OkOrError<List<RoleTypeViewDTO>>> GetRoleTypesAsync();
    public Task<OkOrError<List<SecurityPermissionViewDTO>>> GetSecurityPermissionsAsync();
}
