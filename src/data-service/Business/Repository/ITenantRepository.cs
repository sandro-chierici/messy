using DataService.Business.IO.Tenant;
using DataService.Business.Rules;

namespace DataService.Business.Repository;

public interface ITenantRepository: IRepository    
{
    public Task<OkOrError<TenantView>> GetTenantByIdAsync(int tenantId);
    public Task<OkOrError<TenantView>> GetTenantByTenantIdAsync(string tenantId);
    public Task<OkOrError<string>> CreateTenantAsync(TenantCommand tenantCommand);
    public Task<OkOrError<TenantView>> UpdateTenantByIdAsync(int id, TenantCommand tenantCommand);
    public Task<OkOrError<TenantView>> UpdateTenantByTenantIdAsync(string id, TenantCommand tenantCommand);
    public Task<OkOrError<int>> DeleteTenantByIdAsync(int id);
    public Task<OkOrError<int>> DeleteTenantByTenantIdAsync(string id);
}
