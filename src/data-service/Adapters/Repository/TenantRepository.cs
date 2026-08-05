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

        public async Task<OkOrError<TenantView>> GetTenantByIdAsync(int tenantId)
        {
            try
            {
                using var conn = await _dbConnectionFactory.CreateConnectionAsync();

                var tenant = await conn.QueryFirstOrDefaultAsync<Tenant>(
                    "SELECT * FROM Tenants WHERE Id = @TenantId", 
                    new { TenantId = tenantId });

                if (tenant == null) 
                    return new OkOrError<TenantView>(
                        Ok: false,
                        Error: $"Tenant with ID {tenantId} not found."
                    );

                var exts = await conn.QueryAsync<TenantExt>(
                    "SELECT * FROM TenantExt WHERE TenantId = @TenantId AND IsDeleted = False", 
                    new { TenantId = tenantId });

                return new OkOrError<TenantView>(
                    Ok: true,
                    Value: _entityMapper.MapTenantViewFrom(tenant, exts)
                    );
            }
            catch (Exception ex)
            {
                return new OkOrError<TenantView>(
                    Ok: false,
                    Error: $"An error occurred while retrieving the tenant with ID {tenantId}. Error: {ex.Message}");
            }
        }

        public async Task<OkOrError<TenantView>> GetTenantByTenantIdAsync(string tenantId)
        {
            var tenant = await _dbConnectionFactory.ExecuteAsync(i =>
            {
                return i.QueryFirstOrDefaultAsync<Tenant>("SELECT * FROM Tenants WHERE TenantId = @TenantId", new { TenantId = tenantId });
            });

            if (!tenant || tenant.Value == null)
                return new OkOrError<TenantView>(
                    Ok: false,
                    Error: $"Tenant with TenantID {tenantId} not found. Error: {tenant.Error}");

            var res = tenant.Value;
            return new OkOrError<TenantView>(
                Ok: true,
                Value: _entityMapper.MapTenantViewFrom(res)
                );
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
