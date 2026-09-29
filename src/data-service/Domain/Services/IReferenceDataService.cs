using DataService.Domain.IO.Reference;
using DataService.Domain.Rules;

namespace DataService.Domain.Services;

public interface IReferenceDataService : IBackendService
{
    Task<OkOrError<List<RoleTypeViewDTO>>> GetRoleTypesAsync();
    Task<OkOrError<List<SecurityPermissionViewDTO>>> GetSecurityPermissionsAsync();
}
