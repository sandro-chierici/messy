using DataService.Domain.IO.Tenant;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;

namespace DataService.Domain.Repository;

public interface ITenantRepository: IRepository    
{
    public Task<OkOrError<TenantView>> GetTenantByIdAsync(int tenantId);
    public Task<OkOrError<TenantView>> GetTenantByTenantIdAsync(string tenantId);
    public Task<OkOrError<string>> CreateTenantAsync(Guid tenantId, TenantCommand command);
    public Task<OkOrError<int>> DeleteTenantByIdAsync(int id);
    public Task<OkOrError<int>> DeleteTenantByTenantIdAsync(string id);
}
