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
                    "SELECT * FROM Tenants WHERE Id = @Tenantd",
                    new { TenantId = tenantId });

                if (tenant == null)
                    return new OkOrError<TenantView>(
                        Ok: false,
                        Error: $"Tenant with TenantId {tenantId} not found."
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
