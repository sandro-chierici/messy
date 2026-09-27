using DataService.Domain.IO.Tenant;
using DataService.Domain.Rules;

namespace DataService.Domain.Services
{
    public interface ITenantService: IBackendService
    {
        Task<OkOrError<TenantViewDTO>> GetTenantAsync(string tenantId);
        Task<OkOrError<string>> CreateTenantAsync(TenantCreateDTO tenantCommand);
        Task<OkOrError<TenantViewDTO>> UpdateTenantAsync(string id, TenantCreateDTO tenantCommand);
        Task<OkOrError<int>> DeleteTenantAsync(string id);
    }
}
