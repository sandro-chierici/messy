using Dapper;
using DataService.Domain.IO;
using DataService.Domain.IO.Tenant;
using DataService.Domain.Mapper.Tenant;
using DataService.Domain.Repository;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;
using DataService.Domain.Services.Events;

namespace DataService.Infrastructure.Repository;

public class TenantRepository(
    IDbConnectionFactory dbConnectionFactory,
    IOutboxWriter outboxWriter,
    TenantMapper tenantMapper   
    ) : BaseRepository, ITenantRepository
{

    public async Task<OkOrError<string>> CreateTenantAsync(Guid tenantId, TenantCreateDTO command, EventBase? successEvent = null)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();
            using var transaction = conn.BeginTransaction();

            var model = tenantMapper.MapTenantFrom(command, tenantId);

            var res = await conn.ExecuteAsync(
                @"INSERT INTO tenants 
                             (tenant_id, name, code, legal_name, tax_code, 
                              country, time_zone, locale, industry_type, is_active, 
                              license_type, license_expires_at, max_users, max_machines, 
                              created_utc_date, updated_utc_date, created_by, ext_props)
                      VALUES (@TenantId, @Name, @Code, @LegalName, @TaxCode, @Country, 
                              @TimeZone, @Locale, @IndustryType, @IsActive, @LicenseType, 
                              @LicenseExpiresAtUTC, @MaxUsers, @MaxMachines, @CreatedUTCDate,
                              @UpdatedUTCDate, @CreatedBy, @ExtProps::jsonb)",
                model,
                transaction);

            // the event is committed together with the tenant, or not at all
            if (successEvent != null)
                await outboxWriter.AddAsync(conn, transaction, successEvent);

            transaction.Commit();

            return model.TenantId.ToString();
        }
        catch (Exception ex)
        {
            return new OkOrError<string>(
                    Ok: false,
                    Error: $"Error inserting new Tenant: {ex.Message}"
                ); ;
        }
    }

    public Task<OkOrError<int>> DeleteTenantByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<OkOrError<int>> DeleteTenantByTenantIdAsync(string id)
    {
        throw new NotImplementedException();
    }

    public async Task<OkOrError<TenantViewDTO>> GetTenantByIdAsync(int id)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            var tenant = await conn.QueryFirstOrDefaultAsync<Tenant>(
             @"SELECT id, tenant_id AS TenantId, name, code AS Code, legal_name AS LegalName, tax_code AS TaxCode, country AS Country, time_zone  AS TimeZone,
                             locale AS Locale, industry_type AS IndustryType, is_active AS IsActive, license_type AS LicenseType, license_expires_at  AS LicenseExpiresAtUTC,
                             max_users AS MaxUsers, max_machines AS MaxMachines, created_utc_date AS CreatedUTCDate, updated_utc_date AS UpdatedUTCDate, created_by AS CreatedBy,
                             ext_props AS ExtProps
                  FROM Tenants WHERE id = @Id",
             new { id });

            if (tenant == null)
                return new OkOrError<TenantViewDTO>(
                    Ok: false,
                    Error: $"Tenant with ID {id} not found."
                );

            return new OkOrError<TenantViewDTO>(
                Ok: true,
                Value: tenantMapper.MapTenantViewFrom(tenant)
                );
        }
        catch (Exception ex)
        {
            return new OkOrError<TenantViewDTO>(
                Ok: false,
                Error: $"An error occurred while retrieving the tenant with ID {id}. Error: {ex.Message}");
        }
    }

    public async Task<OkOrError<TenantViewDTO>> GetTenantByTenantIdAsync(string tenantId)
    {
        try
        {
            using var conn = await dbConnectionFactory.CreateConnectionAsync();

            var tenant = await conn.QueryFirstOrDefaultAsync<Tenant>(
                @"SELECT id, tenant_id AS TenantId, name, code AS Code, legal_name AS LegalName, tax_code AS TaxCode, country AS Country, time_zone  AS TimeZone,
                             locale AS Locale, industry_type AS IndustryType, is_active AS IsActive, license_type AS LicenseType, license_expires_at  AS LicenseExpiresAtUTC,
                             max_users AS MaxUsers, max_machines AS MaxMachines, created_utc_date AS CreatedUTCDate, updated_utc_date AS UpdatedUTCDate, created_by AS CreatedBy,
                             ext_props AS ExtProps
                     FROM tenants WHERE tenant_id = @TenantId::uuid",
                new { TenantId = tenantId });

            if (tenant == null)
                return new OkOrError<TenantViewDTO>(
                    Ok: false,
                    Error: $"Tenant with TenantId {tenantId} not found."
                );

            return new OkOrError<TenantViewDTO>(
                Ok: true,
                Value: tenantMapper.MapTenantViewFrom(tenant)
                );
        }
        catch (Exception ex)
        {
            return new OkOrError<TenantViewDTO>(
                Ok: false,
                Error: $"An error occurred while retrieving the tenant with TenantId {tenantId}. Error: {ex.Message}");
        }
    }
}
