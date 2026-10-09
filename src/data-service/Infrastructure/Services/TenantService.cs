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
    IEventPublisher eventPublisher,
    SwissKnife swissKnife) : ITenantService
{
    public async Task<OkOrError<TenantViewDTO>> GetTenantAsync(string tenantId)
        => await tenantRepository.GetTenantByTenantIdAsync(tenantId);


    public async Task<OkOrError<string>> CreateTenantAsync(TenantCreateDTO tenantCommand)
    {
        // create a brand new Id 
        var newTenantId = swissKnife.GenerateGuid();

        // the repository stores this event in the outbox, in the same transaction as the tenant
        var createdEvent = new CreatedEvent
        {
            Id = swissKnife.GenerateGuid(),
            Producer = "TenantService.CreateTenantAsync",
            ResourceName = "Tenant",
            ResourceId = newTenantId.ToString(),
            ResourceType = "Tenant"
        };

        var res = await tenantRepository.CreateTenantAsync(newTenantId, tenantCommand, createdEvent);

        // nothing was committed: the failure is published on its own
        if (!res.Ok)
            await eventPublisher.PublishEventAsync(new FailureEvent
            {
                Id = swissKnife.GenerateGuid(),
                Producer = "TenantService.CreateTenantAsync",
                ResourceName = "Tenant",
                ResourceId = newTenantId.ToString(),
                ResourceType = "Tenant",
                ErrorMessage = res.Error ?? "Unknown error creating tenant"
            });

        return res;
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
