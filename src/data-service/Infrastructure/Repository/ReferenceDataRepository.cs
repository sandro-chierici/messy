using Dapper;
using DataService.Domain.IO;
using DataService.Domain.IO.Reference;
using DataService.Domain.Mapper.Security;
using DataService.Domain.Repository;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;

namespace DataService.Infrastructure.Repository;

public class ReferenceDataRepository(
    IDbConnectionFactory dbConnectionFactory,
    SecurityGroupMapper securityGroupMapper
    ) : BaseRepository, IReferenceDataRepository
{
    public async Task<OkOrError<List<RoleTypeViewDTO>>> GetRoleTypesAsync()
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            var roleTypes = await conn.QueryAsync<RoleType>(
                @"SELECT rt.id, rt.code, rt.parent_pk, parent.code AS parent_code, rt.description
                  FROM role_types rt
                       LEFT JOIN role_types parent ON parent.id = rt.parent_pk
                  ORDER BY rt.code");

            return roleTypes.Select(securityGroupMapper.MapRoleTypeViewFrom).ToList();
        }
        catch (Exception ex)
        {
            return new OkOrError<List<RoleTypeViewDTO>>(false, Error: $"An error occurred while retrieving the role types. Error: {ex.Message}");
        }
    }

    public async Task<OkOrError<List<SecurityPermissionViewDTO>>> GetSecurityPermissionsAsync()
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            var permissions = await conn.QueryAsync<SecurityPermission>(
                "SELECT id, code, description FROM security_permissions ORDER BY code");

            return permissions.Select(securityGroupMapper.MapSecurityPermissionViewFrom).ToList();
        }
        catch (Exception ex)
        {
            return new OkOrError<List<SecurityPermissionViewDTO>>(false, Error: $"An error occurred while retrieving the security permissions. Error: {ex.Message}");
        }
    }
}
