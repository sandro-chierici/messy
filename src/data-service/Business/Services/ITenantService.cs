using DataService.Business.IO.Tenant;
using DataService.Business.Rules;

namespace DataService.Business.Services
{
    public interface ITenantService: IBackendService
    {
        Task<OkOrError<TenantView>> GetTenantAsync(string tenantId);
        Task<OkOrError<string>> CreateTenantAsync(TenantCommand tenantCommand);
        Task<OkOrError<TenantView>> UpdateTenantAsync(string id, TenantCommand tenantCommand);
        Task<OkOrError<int>> DeleteTenantAsync(string id);
    }
}
