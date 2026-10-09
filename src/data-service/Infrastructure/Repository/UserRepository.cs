using Dapper;
using DataService.Domain.IO;
using DataService.Domain.IO.User;
using DataService.Domain.Mapper.User;
using DataService.Domain.Repository;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;
using DataService.Domain.Services.Events;
using Npgsql;
using System.Data;

namespace DataService.Infrastructure.Repository;

public class UserRepository(
    IDbConnectionFactory dbConnectionFactory,
    IOutboxWriter outboxWriter,
    UserMapper userMapper
    ) : BaseRepository, IUserRepository
{
    /// <summary>
    /// Internal primary keys of a user. They never leave this class.
    /// </summary>
    private sealed record UserKeys(int PartyPk, int UserLoginPk, int TenantPk);
    private sealed record RoleRow(Guid PartyId, string Code);
    private sealed record GroupRow(Guid PartyId, Guid SecurityGroupId);

    private const string DetailSelect =
        @"SELECT p.party_id, t.tenant_id, ul.user_login_id, ul.username, ul.enabled, ul.is_system,
                 ul.require_password_change, ul.last_locale, ul.last_time_zone,
                 p.external_id, p.description, p.is_active, p.created_utc_date, p.updated_utc_date,
                 p.created_by, p.ext_props,
                 pe.salutation, pe.first_name, pe.middle_name, pe.last_name, pe.nickname,
                 pe.personal_title, pe.suffix, pe.gender, pe.birth_date, pe.comments
          FROM parties p
               JOIN tenants t      ON t.id = p.tenant_pk
               JOIN persons pe     ON pe.party_pk = p.id
               JOIN user_logins ul ON ul.party_pk = p.id ";

    public async Task<OkOrError<string>> CreateUserAsync(
        Guid tenantId, 
        Guid partyId, 
        Guid userLoginId, 
        UserCreateDTO command,
        IReadOnlyCollection<Guid> securityGroupIds, 
        string passwordHash,
        EventBase? successEvent = null)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var tenantPk = await conn.ExecuteScalarAsync<int?>(
                "SELECT id FROM tenants WHERE tenant_id = @TenantId",
                new { TenantId = tenantId }, 
                transaction);

            if (tenantPk == null)
                return new OkOrError<string>(false, Error: $"Tenant with TenantId {tenantId} not found.");

            var partyTypePk = await conn.ExecuteScalarAsync<int>(
                "SELECT id FROM party_types WHERE code = 'PERSON'", 
                transaction: transaction);

            var party = userMapper.MapPartyFrom(command, partyId);
            party.TenantPk = tenantPk.Value;
            party.PartyTypePk = partyTypePk;

            var partyPk = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO parties
                         (party_id, tenant_pk, party_type_pk, external_id, description, is_active,
                          created_utc_date, updated_utc_date, created_by, ext_props)
                  VALUES (@PartyId, @TenantPk, @PartyTypePk, @ExternalId, @Description, @IsActive,
                          @CreatedUTCDate, @UpdatedUTCDate, @CreatedBy, @ExtProps::jsonb)
                  RETURNING id",
                party, 
                transaction);

            var person = userMapper.MapPersonFrom(command);
            person.PartyPk = partyPk;
            await conn.ExecuteAsync(
                @"INSERT INTO persons
                         (party_pk, salutation, first_name, middle_name, last_name, nickname,
                          personal_title, suffix, gender, birth_date, comments)
                  VALUES (@PartyPk, @Salutation, @FirstName, @MiddleName, @LastName, @Nickname,
                          @PersonalTitle, @Suffix, @Gender, @BirthDate, @Comments)",
                person, 
                transaction);

            var login = userMapper.MapUserLoginFrom(command, userLoginId, passwordHash);
            login.PartyPk = partyPk;
            login.TenantPk = tenantPk.Value;
            var loginPk = await conn.ExecuteScalarAsync<int>(
                @"INSERT INTO user_logins
                         (user_login_id, party_pk, tenant_pk, username, password_hash, enabled, is_system,
                          require_password_change, disabled_utc_date, last_locale, last_time_zone,
                          external_auth_id, created_utc_date, created_by)
                  VALUES (@UserLoginId, @PartyPk, @TenantPk, @Username, @PasswordHash, @Enabled, @IsSystem,
                          @RequirePasswordChange, @DisabledUTCDate, @LastLocale, @LastTimeZone,
                          @ExternalAuthId, @CreatedUTCDate, @CreatedBy)
                  RETURNING id",
                login, 
                transaction);

            var roles = await ReplaceRolesAsync(conn, transaction, partyPk, command.RoleCodes ?? []);
            if (!roles) return new OkOrError<string>(false, Error: roles.Error);

            var groups = await ReplaceSecurityGroupsAsync(conn, transaction, loginPk, tenantPk.Value, securityGroupIds);
            if (!groups) return new OkOrError<string>(false, Error: groups.Error);

            // the event is committed together with the user, or not at all
            if (successEvent != null)
                await outboxWriter.AddAsync(conn, transaction, successEvent);

            transaction.Commit();
            return partyId.ToString();
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return new OkOrError<string>(false, Error: $"Username '{command.Username}' already exists in this tenant.");
        }
        catch (Exception ex)
        {
            return new OkOrError<string>(false, Error: $"Error inserting new User: {ex.Message}");
        }
    }

    public async Task<OkOrError<UserViewDTO>> GetUserAsync(Guid tenantId, Guid userId)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            return await GetUserCoreAsync(conn, null, tenantId, userId);
        }
        catch (Exception ex)
        {
            return new OkOrError<UserViewDTO>(false, Error: $"An error occurred while retrieving the user {userId}. Error: {ex.Message}");
        }
    }

    public async Task<OkOrError<QueryResult>> ListUsersAsync(Guid tenantId, int page, int pageSize)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            var tenantExists = await conn.ExecuteScalarAsync<bool>(
                "SELECT EXISTS (SELECT 1 FROM tenants WHERE tenant_id = @TenantId)", new { TenantId = tenantId });
            if (!tenantExists)
                return new OkOrError<QueryResult>(false, Error: $"Tenant with TenantId {tenantId} not found.");

            var total = await conn.ExecuteScalarAsync<int>(
                @"SELECT count(*) FROM parties p
                       JOIN tenants t      ON t.id = p.tenant_pk
                       JOIN user_logins ul ON ul.party_pk = p.id
                  WHERE t.tenant_id = @TenantId",
                new { TenantId = tenantId });

            var details = (await conn.QueryAsync<UserDetail>(
                DetailSelect + "WHERE t.tenant_id = @TenantId ORDER BY p.id LIMIT @PageSize OFFSET @Offset",
                new { TenantId = tenantId, PageSize = pageSize, Offset = (page - 1) * pageSize })).ToList();

            var ids = details.Select(d => d.PartyId).ToArray();
            var roles = (await LoadRolesAsync(conn, null, ids)).ToLookup(r => r.PartyId, r => r.Code);
            var groups = (await LoadGroupsAsync(conn, null, ids)).ToLookup(g => g.PartyId, g => g.SecurityGroupId);

            var items = details
                .Select(d => (object)userMapper.MapUserViewFrom(d, roles[d.PartyId], groups[d.PartyId]))
                .ToArray();

            return QueryResult.Create(total, items);
        }
        catch (Exception ex)
        {
            return new OkOrError<QueryResult>(false, Error: $"An error occurred while listing the users of tenant {tenantId}. Error: {ex.Message}");
        }
    }

    public async Task<OkOrError<UserViewDTO>> UpdateUserAsync(Guid tenantId, Guid userId, UserUpdateDTO command, string? passwordHash)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var keys = await ResolveKeysAsync(conn, transaction, tenantId, userId);
            if (keys == null)
                return new OkOrError<UserViewDTO>(false, Error: $"User {userId} not found in tenant {tenantId}.");

            var party = userMapper.MapPartyFrom(command, userId);
            party.Id = keys.PartyPk;
            await conn.ExecuteAsync(
                @"UPDATE parties
                     SET external_id = @ExternalId, description = @Description, is_active = @IsActive,
                         updated_utc_date = @UpdatedUTCDate, ext_props = @ExtProps::jsonb
                   WHERE id = @Id",
                party, transaction);

            var person = userMapper.MapPersonFrom(command);
            person.PartyPk = keys.PartyPk;
            await conn.ExecuteAsync(
                @"UPDATE persons
                     SET salutation = @Salutation, first_name = @FirstName, middle_name = @MiddleName,
                         last_name = @LastName, nickname = @Nickname, personal_title = @PersonalTitle,
                         suffix = @Suffix, gender = @Gender, birth_date = @BirthDate, comments = @Comments
                   WHERE party_pk = @PartyPk",
                person, transaction);

            await conn.ExecuteAsync(
                @"UPDATE user_logins
                     SET enabled = @Enabled, require_password_change = @RequirePasswordChange,
                         last_locale = @LastLocale, last_time_zone = @LastTimeZone,
                         disabled_utc_date = CASE WHEN @Enabled THEN NULL ELSE COALESCE(disabled_utc_date, @Now) END,
                         updated_utc_date = @Now
                   WHERE id = @Id",
                new
                {
                    command.Enabled,
                    command.RequirePasswordChange,
                    command.LastLocale,
                    command.LastTimeZone,
                    Now = DateTime.UtcNow,
                    Id = keys.UserLoginPk
                }, transaction);

            if (passwordHash != null)
                await conn.ExecuteAsync(
                    "UPDATE user_logins SET password_hash = @PasswordHash, successive_failed_logins = 0 WHERE id = @Id",
                    new { PasswordHash = passwordHash, Id = keys.UserLoginPk }, transaction);

            var res = await GetUserCoreAsync(conn, transaction, tenantId, userId);
            transaction.Commit();
            return res;
        }
        catch (Exception ex)
        {
            return new OkOrError<UserViewDTO>(false, Error: $"Error updating user {userId}: {ex.Message}");
        }
    }

    public async Task<OkOrError<int>> DisableUserAsync(Guid tenantId, Guid userId)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var keys = await ResolveKeysAsync(conn, transaction, tenantId, userId);
            if (keys == null)
                return new OkOrError<int>(false, Error: $"User {userId} not found in tenant {tenantId}.");

            var now = DateTime.UtcNow;
            await conn.ExecuteAsync(
                @"UPDATE user_logins
                     SET enabled = FALSE, disabled_utc_date = COALESCE(disabled_utc_date, @Now), updated_utc_date = @Now
                   WHERE id = @Id",
                new { Now = now, Id = keys.UserLoginPk }, transaction);
            await conn.ExecuteAsync(
                "UPDATE parties SET is_active = FALSE, updated_utc_date = @Now WHERE id = @Id",
                new { Now = now, Id = keys.PartyPk }, transaction);

            transaction.Commit();
            return 1;
        }
        catch (Exception ex)
        {
            return new OkOrError<int>(false, Error: $"Error disabling user {userId}: {ex.Message}");
        }
    }

    public async Task<OkOrError<int>> SetUserRolesAsync(Guid tenantId, Guid userId, IReadOnlyCollection<string> roleCodes)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var keys = await ResolveKeysAsync(conn, transaction, tenantId, userId);
            if (keys == null)
                return new OkOrError<int>(false, Error: $"User {userId} not found in tenant {tenantId}.");

            var res = await ReplaceRolesAsync(conn, transaction, keys.PartyPk, roleCodes);
            if (!res) return res;

            transaction.Commit();
            return res;
        }
        catch (Exception ex)
        {
            return new OkOrError<int>(false, Error: $"Error setting roles of user {userId}: {ex.Message}");
        }
    }

    public async Task<OkOrError<int>> SetUserSecurityGroupsAsync(Guid tenantId, Guid userId, IReadOnlyCollection<Guid> securityGroupIds)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var keys = await ResolveKeysAsync(conn, transaction, tenantId, userId);
            if (keys == null)
                return new OkOrError<int>(false, Error: $"User {userId} not found in tenant {tenantId}.");

            var res = await ReplaceSecurityGroupsAsync(conn, transaction, keys.UserLoginPk, keys.TenantPk, securityGroupIds);
            if (!res) return res;

            transaction.Commit();
            return res;
        }
        catch (Exception ex)
        {
            return new OkOrError<int>(false, Error: $"Error setting security groups of user {userId}: {ex.Message}");
        }
    }

    public async Task<OkOrError<int>> CountEnabledUsersAsync(Guid tenantId)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            return await conn.ExecuteScalarAsync<int>(
                @"SELECT count(*) FROM user_logins ul JOIN tenants t ON t.id = ul.tenant_pk
                  WHERE t.tenant_id = @TenantId AND ul.enabled",
                new { TenantId = tenantId });
        }
        catch (Exception ex)
        {
            return new OkOrError<int>(false, Error: $"Error counting users of tenant {tenantId}: {ex.Message}");
        }
    }

    private static Task<UserKeys?> ResolveKeysAsync(IDbConnection conn, IDbTransaction? tx, Guid tenantId, Guid userId) =>
        conn.QueryFirstOrDefaultAsync<UserKeys>(
            @"SELECT p.id AS PartyPk, ul.id AS UserLoginPk, p.tenant_pk AS TenantPk
              FROM parties p
                   JOIN tenants t      ON t.id = p.tenant_pk
                   JOIN user_logins ul ON ul.party_pk = p.id
              WHERE t.tenant_id = @TenantId AND p.party_id = @UserId",
            new { TenantId = tenantId, UserId = userId }, tx);

    private async Task<OkOrError<UserViewDTO>> GetUserCoreAsync(IDbConnection conn, IDbTransaction? tx, Guid tenantId, Guid userId)
    {
        var detail = await conn.QueryFirstOrDefaultAsync<UserDetail>(
            DetailSelect + "WHERE t.tenant_id = @TenantId AND p.party_id = @UserId",
            new { TenantId = tenantId, UserId = userId }, tx);

        if (detail == null)
            return new OkOrError<UserViewDTO>(false, Error: $"User {userId} not found in tenant {tenantId}.");

        var ids = new[] { userId };
        var roles = await LoadRolesAsync(conn, tx, ids);
        var groups = await LoadGroupsAsync(conn, tx, ids);

        return userMapper.MapUserViewFrom(detail, roles.Select(r => r.Code), groups.Select(g => g.SecurityGroupId));
    }

    private static async Task<IEnumerable<RoleRow>> LoadRolesAsync(IDbConnection conn, IDbTransaction? tx, Guid[] partyIds) =>
        partyIds.Length == 0
            ? []
            : await conn.QueryAsync<RoleRow>(
                @"SELECT p.party_id AS PartyId, rt.code AS Code
                  FROM party_roles pr
                       JOIN parties p     ON p.id = pr.party_pk
                       JOIN role_types rt ON rt.id = pr.role_type_pk
                  WHERE p.party_id = ANY(@Ids)
                  ORDER BY rt.code",
                new { Ids = partyIds }, tx);

    private static async Task<IEnumerable<GroupRow>> LoadGroupsAsync(IDbConnection conn, IDbTransaction? tx, Guid[] partyIds) =>
        partyIds.Length == 0
            ? []
            : await conn.QueryAsync<GroupRow>(
                @"SELECT p.party_id AS PartyId, sg.security_group_id AS SecurityGroupId
                  FROM user_login_security_groups ulsg
                       JOIN user_logins ul     ON ul.id = ulsg.user_login_pk
                       JOIN parties p          ON p.id = ul.party_pk
                       JOIN security_groups sg ON sg.id = ulsg.security_group_pk
                  WHERE p.party_id = ANY(@Ids) AND (ulsg.thru_date IS NULL OR ulsg.thru_date > now())
                  ORDER BY sg.name",
                new { Ids = partyIds }, tx);

    /// <summary>
    /// Replaces the roles of a party with the given role codes. Fails on unknown codes.
    /// </summary>
    private async Task<OkOrError<int>> ReplaceRolesAsync(IDbConnection conn, 
        IDbTransaction tx, 
        int partyPk, 
        IReadOnlyCollection<string> roleCodes)
    {
        var codes = roleCodes.Select(c => c.Trim().ToUpperInvariant()).Distinct().ToArray();

        await conn.ExecuteAsync("DELETE FROM party_roles WHERE party_pk = @PartyPk", 
            new { PartyPk = partyPk }, 
            tx);

        if (codes.Length == 0) return 0;

        var inserted = await conn.ExecuteAsync(
            "INSERT INTO party_roles (party_pk, role_type_pk) SELECT @PartyPk, id FROM role_types WHERE code = ANY(@Codes)",
            new { PartyPk = partyPk, Codes = codes },
            tx);

        if (inserted != codes.Length)
            return new OkOrError<int>(false, Error: "One or more role codes are unknown.");

        return inserted;
    }

    /// <summary>
    /// Replaces the active security groups of a login. Groups must be built-in or belong to the tenant.
    /// Removed assignments are closed with thru_date, as in OFBiz.
    /// </summary>
    private async Task<OkOrError<int>> ReplaceSecurityGroupsAsync(IDbConnection conn, 
        IDbTransaction tx, 
        int userLoginPk, 
        int tenantPk, 
        IReadOnlyCollection<Guid> securityGroupIds)
    {
        var ids = securityGroupIds.Distinct().ToArray();

        var groupPks = ids.Length == 0
            ? []
            : (await conn.QueryAsync<int>(
                @"SELECT id FROM security_groups
                  WHERE security_group_id = ANY(@Ids) AND (tenant_pk IS NULL OR tenant_pk = @TenantPk)",
                new { Ids = ids, TenantPk = tenantPk }, tx)).ToArray();

        if (groupPks.Length != ids.Length)
            return new OkOrError<int>(false, Error: "One or more security groups are unknown for this tenant.");

        if (groupPks.Length == 0)
        {
            await conn.ExecuteAsync(
                @"UPDATE user_login_security_groups SET thru_date = now()
                  WHERE user_login_pk = @UserLoginPk AND (thru_date IS NULL OR thru_date > now())",
                new { UserLoginPk = userLoginPk }, tx);
            return 0;
        }

        await conn.ExecuteAsync(
            @"UPDATE user_login_security_groups SET thru_date = now()
              WHERE user_login_pk = @UserLoginPk AND (thru_date IS NULL OR thru_date > now())
                AND security_group_pk <> ALL(@GroupPks)",
            new { UserLoginPk = userLoginPk, GroupPks = groupPks }, tx);

        await conn.ExecuteAsync(
            @"INSERT INTO user_login_security_groups (user_login_pk, security_group_pk, from_date)
              SELECT @UserLoginPk, g.id, now() FROM security_groups g
              WHERE g.id = ANY(@GroupPks)
                AND NOT EXISTS (SELECT 1 FROM user_login_security_groups x
                                WHERE x.user_login_pk = @UserLoginPk AND x.security_group_pk = g.id
                                  AND (x.thru_date IS NULL OR x.thru_date > now()))",
            new { UserLoginPk = userLoginPk, GroupPks = groupPks }, tx);

        return groupPks.Length;
    }
}
