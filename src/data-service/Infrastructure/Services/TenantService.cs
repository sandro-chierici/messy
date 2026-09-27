using DataService.Domain.IO.Tenant;
using DataService.Domain.Repository;
using DataService.Domain.Rules;
using DataService.Domain.Services;
using DataService.Domain.Services.Events;
using DataService.Domain.Tools;

namespace DataService.Infrastructure.Services;

/// <summary>
/// Scoped service
/// </summary>
/// <param name="tenantRepository"></param>
/// <param name="swissKnife"></param>
public class TenantService(
    ITenantRepository tenantRepository, 
    SwissKnife swissKnife) : ITenantService
{
    public async Task<OkOrError<TenantViewDTO>> GetTenantAsync(string tenantId)
        => await tenantRepository.GetTenantByTenantIdAsync(tenantId);


    public async Task<OkOrError<string>> CreateTenantAsync(TenantCreateDTO tenantCommand)
    {
        // create a brand new Id 
        var newTenantId = swissKnife.GenerateGuid();

        var res = await tenantRepository.CreateTenantAsync(newTenantId, tenantCommand);

        var ev = res switch
        {
            OkOrError<string> { Ok: true } => new CreatedEvent 
            {
                Producer = "TenantService.CreateTenantAsync",
                ResourceName = "Tenant",
                ResourceId = newTenantId.ToString(),
                ResourceType = "Tenant"
            },

            OkOrError<string> { Ok: false, Error: var error } => new FailureEvent 
            { 
                Producer = "TenantService.CreateTenantAsync",
                ResourceName = "Tenant",
                ResourceId = newTenantId.ToString(),
                ResourceType = "Tenant",
                ErrorMessage = error ?? "Unknown error creating tenant"
            },
            _ => null
        };

        return newTenantId.ToString();
    }

    public async Task<OkOrError<TenantViewDTO>> UpdateTenantAsync(string id, TenantCreateDTO tenantCommand)
    {
        throw new NotImplementedException();
    }

    public async Task<OkOrError<int>> DeleteTenantAsync(string id)
    {
        throw new NotImplementedException();
    }
}
