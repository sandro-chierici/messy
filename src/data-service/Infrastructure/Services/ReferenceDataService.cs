using DataService.Domain.IO.Reference;
using DataService.Domain.Repository;
using DataService.Domain.Rules;
using DataService.Domain.Services;

namespace DataService.Infrastructure.Services;

/// <summary>
/// Scoped service
/// </summary>
public class ReferenceDataService(
    IReferenceDataRepository referenceDataRepository) : IReferenceDataService
{
    public async Task<OkOrError<List<RoleTypeViewDTO>>> GetRoleTypesAsync()
        => await referenceDataRepository.GetRoleTypesAsync();

    public async Task<OkOrError<List<SecurityPermissionViewDTO>>> GetSecurityPermissionsAsync()
        => await referenceDataRepository.GetSecurityPermissionsAsync();
}
