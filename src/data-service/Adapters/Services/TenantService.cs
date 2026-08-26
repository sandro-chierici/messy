using DataService.Business.IO.Tenant;
using DataService.Business.Repository;
using DataService.Business.Rules;
using DataService.Business.Services;
using DataService.Business.Tools;

namespace DataService.Adapters.Services;

public class TenantService(
    ITenantRepository tenantRepository, 
    SwissKnife swissKnife) : ITenantService
{
    public async Task<OkOrError<TenantView>> GetTenantAsync(string tenantId)
        => await tenantRepository.GetTenantByTenantIdAsync(tenantId);


    public async Task<OkOrError<string>> CreateTenantAsync(TenantCommand tenantCommand)
    {
        // create a brand new Id 
        var newTenantId = swissKnife.GenerateGuid();

        // send request by queue to the repository to create a new tenant

        return newTenantId.ToString();
    }

    public async Task<OkOrError<TenantView>> UpdateTenantAsync(string id, TenantCommand tenantCommand)
    {
        throw new NotImplementedException();
    }

    public async Task<OkOrError<int>> DeleteTenantAsync(string id)
    {
        throw new NotImplementedException();
    }
}
