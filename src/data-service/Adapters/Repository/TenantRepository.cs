using Dapper;
using DataService.Business.IO;
using DataService.Business.IO.DataCommand;
using DataService.Business.IO.DataView;
using DataService.Business.IO.Mapper;
using DataService.Business.Repository;
using DataService.Business.Repository.Entity.Tenant;
using DataService.Business.Rules;

namespace DataService.Adapters.Repository
{
    public class TenantRepository(IDbConnectionFactory dbConnectionFactory, EntityMapper entityMapper) : ITenantRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
        private readonly EntityMapper _entityMapper = entityMapper;

        public Task<OkOrError<TenantView>> CreateTenantAsync(TenantCommand tenantCommand)
        {
            throw new NotImplementedException();
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
                using var conn = await _dbConnectionFactory.CreateConnectionAsync();

                var tenant = await conn.QueryFirstOrDefaultAsync<Tenant>(
                    "SELECT * FROM Tenants WHERE Id = @Id",
                    new { id });

                if (tenant == null)
                    return new OkOrError<TenantView>(
                        Ok: false,
                        Error: $"Tenant with ID {id} not found."
                    );

                var exts = await conn.QueryAsync<TenantExt>(
                    "SELECT * FROM TenantExt WHERE TenantId = @TenantId AND IsDeleted = False",
                    new { tenant.TenantId });

                return new OkOrError<TenantView>(
                    Ok: true,
                    Value: _entityMapper.MapTenantViewFrom(tenant, exts)
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
                using var conn = await _dbConnectionFactory.CreateConnectionAsync();

                var tenant = await conn.QueryFirstOrDefaultAsync<Tenant>(
                    @"SELECT id, tenant_id AS TenantId, name, code AS Code, legal_name AS LegalName, tax_code AS TaxCode, country AS Country, time_zone  AS TimeZone,
                             locale AS Locale, industry_type AS IndustryType, is_active AS IsActive, license_type AS LicenseType, license_expires_at  AS LicenseExpiresAtUTC,
                             max_users AS MaxUsers, max_machines AS MaxMachines, created_utc_date AS CreatedUTCDate, updated_utc_date AS UpdatedUTCDate, created_by AS CreatedBy
                     FROM tenants WHERE tenant_id = @TenantId::uuid",
                    new { TenantId = tenantId });

                if (tenant == null)
                    return new OkOrError<TenantView>(
                        Ok: false,
                        Error: $"Tenant with TenantId {tenantId} not found."
                    );

                var exts = await conn.QueryAsync<TenantExt>(
                    @"SELECT id, tenant_id AS TenantId, name AS Name, type AS Type, value AS Value, is_deleted AS IsDeleted 
                      FROM tenants_ext WHERE tenant_id = @TenantId::uuid AND is_deleted = False",
                    new { tenant.TenantId });

                return new OkOrError<TenantView>(
                    Ok: true,
                    Value: _entityMapper.MapTenantViewFrom(tenant, exts)
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
