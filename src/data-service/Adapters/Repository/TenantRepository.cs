using Dapper;
using DataService.Business.IO;
using DataService.Business.IO.DataCommand;
using DataService.Business.IO.DataView;
using DataService.Business.IO.Mapper;
using DataService.Business.Repository;
using DataService.Business.Repository.Entity.Tenant;
using DataService.Business.Rules;
using DataService.Business.Tools;
using System.Data;
using System.Text.Json;

namespace DataService.Adapters.Repository
{
    public class TenantRepository(
        IDbConnectionFactory dbConnectionFactory,
        EntityMapper entityMapper,
        SwissKnife swissKnife) : ITenantRepository
    {

        public async Task<OkOrError<TenantView>> CreateTenantAsync(TenantCommand tenantCommand)
        {
            try
            {
                using var conn = await dbConnectionFactory.CreateConnectionAsync();
                using var transaction = conn.BeginTransaction();

                var tenantId = swissKnife.GenerateGuid();

                var res = await conn.ExecuteAsync(
                    @"INSERT INTO tenants 
                             (tenant_id, name, code, legal_name, tax_code, country, time_zone, locale, industry_type, is_active)
                      VALUES (@TenantId, @Name, @Code, @LegalName, @TaxCode, @Country, @TimeZone, @Locale, @IndustryType, @IsActive)",
                    new
                    {
                        TenantId = tenantId,
                        tenantCommand.Name,
                        tenantCommand.Code,
                        tenantCommand.LegalName,
                        tenantCommand.TaxCode,
                        tenantCommand.Country,
                        tenantCommand.TimeZone,
                        tenantCommand.Locale,
                        tenantCommand.IndustryType,
                        tenantCommand.IsActive
                    },
                    transaction);

                transaction.Commit();

                var tenantView = new TenantView
                {
                    TenantId = $"{tenantId}",
                    Name = tenantCommand.Name,
                    Code = tenantCommand.Code,
                    LegalName = tenantCommand.LegalName,
                    TaxCode = tenantCommand.TaxCode,
                    Country = tenantCommand.Country,
                    TimeZone = tenantCommand.TimeZone,
                    Locale = tenantCommand.Locale,
                    IndustryType = tenantCommand.IndustryType,
                    IsActive = tenantCommand.IsActive
                };

                return tenantView;
            }
            catch (Exception ex)
            {
                return new OkOrError<TenantView>(
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

        public Task<OkOrError<TenantView>> GetTenantByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public async Task<OkOrError<TenantView>> GetTenantByIdAsync(int id)
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
                    return new OkOrError<TenantView>(
                        Ok: false,
                        Error: $"Tenant with ID {id} not found."
                    );

                return new OkOrError<TenantView>(
                    Ok: true,
                    Value: entityMapper.MapTenantViewFrom(tenant)
                    );
            }
            catch (Exception ex)
            {
                return new OkOrError<TenantView>(
                    Ok: false,
                    Error: $"An error occurred while retrieving the tenant with ID {id}. Error: {ex.Message}");
            }
        }

        public async Task<OkOrError<TenantView>> GetTenantByTenantIdAsync(string tenantId)
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
                    return new OkOrError<TenantView>(
                        Ok: false,
                        Error: $"Tenant with TenantId {tenantId} not found."
                    );

                return new OkOrError<TenantView>(
                    Ok: true,
                    Value: entityMapper.MapTenantViewFrom(tenant)
                    );
            }
            catch (Exception ex)
            {
                return new OkOrError<TenantView>(
                    Ok: false,
                    Error: $"An error occurred while retrieving the tenant with TenantId {tenantId}. Error: {ex.Message}");
            }
        }

        public Task<OkOrError<TenantView>> UpdateTenantByIdAsync(int id, TenantCommand tenantCommand)
        {
            throw new NotImplementedException();
        }

        public Task<OkOrError<TenantView>> UpdateTenantByTenantIdAsync(string id, TenantCommand tenantCommand)
        {
            throw new NotImplementedException();
        }
    }
}
