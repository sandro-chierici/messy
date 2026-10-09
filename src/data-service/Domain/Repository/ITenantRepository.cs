using DataService.Domain.IO.Tenant;
using DataService.Domain.Repository.Models;
using DataService.Domain.Rules;
using DataService.Domain.Services.Events;

namespace DataService.Domain.Repository;

public interface ITenantRepository: IRepository    
{
    public Task<OkOrError<TenantViewDTO>> GetTenantByIdAsync(int tenantId);
    public Task<OkOrError<TenantViewDTO>> GetTenantByTenantIdAsync(string tenantId);
    public Task<OkOrError<string>> CreateTenantAsync(Guid tenantId, TenantCreateDTO command, EventBase? successEvent = null);
    public Task<OkOrError<int>> DeleteTenantByIdAsync(int id);
    public Task<OkOrError<int>> DeleteTenantByTenantIdAsync(string id);
}
