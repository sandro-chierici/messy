using Dapper;
using DataService.Domain.IO;
using DataService.Domain.IO.Security;
using DataService.Domain.Mapper.Security;
using DataService.Domain.Repository;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;
using Npgsql;
using System.Data;

namespace DataService.Infrastructure.Repository;

public class SecurityGroupRepository(
    IDbConnectionFactory dbConnectionFactory,
    SecurityGroupMapper securityGroupMapper
    ) : ISecurityGroupRepository
{
    private sealed record PermissionRow(Guid SecurityGroupId, string Code);

    private const string GroupSelect =
        @"SELECT sg.security_group_id, t.tenant_id, sg.name, sg.description
          FROM security_groups sg
               LEFT JOIN tenants t ON t.id = sg.tenant_pk ";

    public async Task<OkOrError<string>> CreateSecurityGroupAsync(Guid tenantId, Guid securityGroupId, SecurityGroupCreateDTO command)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var groupPk = await conn.ExecuteScalarAsync<int?>(
                @"INSERT INTO security_groups (security_group_id, tenant_pk, name, description)
                  SELECT @SecurityGroupId, t.id, @Name, @Description FROM tenants t WHERE t.tenant_id = @TenantId
                  RETURNING id",
                new { SecurityGroupId = securityGroupId, TenantId = tenantId, command.Name, command.Description },
                transaction);

            if (groupPk == null)
                return new OkOrError<string>(false, Error: $"Tenant with TenantId {tenantId} not found.");

            var codes = (command.PermissionCodes ?? []).Select(c => c.Trim().ToUpperInvariant()).Distinct().ToArray();
            if (codes.Length > 0)
            {
                var inserted = await conn.ExecuteAsync(
                    @"INSERT INTO security_group_permissions (security_group_pk, security_permission_pk)
                      SELECT @GroupPk, id FROM security_permissions WHERE code IN @Codes",
                    new { GroupPk = groupPk.Value, Codes = codes }, transaction);

                if (inserted != codes.Length)
                    return new OkOrError<string>(false, Error: "One or more permission codes are unknown.");
            }

            transaction.Commit();
            return securityGroupId.ToString();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return new OkOrError<string>(false, Error: $"Security group '{command.Name}' already exists in this tenant.");
        }
        catch (Exception ex)
        {
            return new OkOrError<string>(false, Error: $"Error inserting new SecurityGroup: {ex.Message}");
        }
    }

    public async Task<OkOrError<SecurityGroupViewDTO>> GetSecurityGroupAsync(Guid tenantId, Guid securityGroupId)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            // visible: built-in groups and groups of the requested tenant
            var group = await conn.QueryFirstOrDefaultAsync<SecurityGroup>(
                GroupSelect + "WHERE sg.security_group_id = @SecurityGroupId AND (sg.tenant_pk IS NULL OR t.tenant_id = @TenantId)",
                new { SecurityGroupId = securityGroupId, TenantId = tenantId });

            if (group == null)
                return new OkOrError<SecurityGroupViewDTO>(false, Error: $"Security group {securityGroupId} not found.");

            var permissions = await LoadPermissionsAsync(conn, [securityGroupId]);
            return securityGroupMapper.MapSecurityGroupViewFrom(group, permissions.Select(p => p.Code));
        }
        catch (Exception ex)
        {
            return new OkOrError<SecurityGroupViewDTO>(false, Error: $"An error occurred while retrieving the security group {securityGroupId}. Error: {ex.Message}");
        }
    }

    public async Task<OkOrError<List<SecurityGroupViewDTO>>> ListSecurityGroupsAsync(Guid tenantId)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            var tenantExists = await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM tenants WHERE tenant_id = @TenantId)", new { TenantId = tenantId });
            if (!tenantExists)
                return new OkOrError<List<SecurityGroupViewDTO>>(false, Error: $"Tenant with TenantId {tenantId} not found.");

            var groups = (await conn.QueryAsync<SecurityGroup>(
                GroupSelect + "WHERE sg.tenant_pk IS NULL OR t.tenant_id = @TenantId ORDER BY sg.name",
                new { TenantId = tenantId })).ToList();

            var permissions = (await LoadPermissionsAsync(conn, groups.Select(g => g.SecurityGroupId).ToArray()))
                .ToLookup(p => p.SecurityGroupId, p => p.Code);

            return groups
                .Select(g => securityGroupMapper.MapSecurityGroupViewFrom(g, permissions[g.SecurityGroupId]))
                .ToList();
        }
        catch (Exception ex)
        {
            return new OkOrError<List<SecurityGroupViewDTO>>(false, Error: $"An error occurred while listing the security groups of tenant {tenantId}. Error: {ex.Message}");
        }
    }

    private static async Task<IEnumerable<PermissionRow>> LoadPermissionsAsync(IDbConnection conn, Guid[] groupIds) =>
        groupIds.Length == 0
            ? []
            : await conn.QueryAsync<PermissionRow>(
                @"SELECT sg.security_group_id AS SecurityGroupId, sp.code AS Code
                  FROM security_group_permissions sgp
                       JOIN security_groups sg      ON sg.id = sgp.security_group_pk
                       JOIN security_permissions sp ON sp.id = sgp.security_permission_pk
                  WHERE sg.security_group_id IN @Ids
                  ORDER BY sp.code",
                new { Ids = groupIds });
}
