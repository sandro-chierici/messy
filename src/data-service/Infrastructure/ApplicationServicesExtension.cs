using DataService.Domain.IO;
using DataService.Domain.Mapper;
using DataService.Domain.Mapper.Tenant;
using DataService.Domain.Repository;
using DataService.Domain.Rules;
using DataService.Domain.Services;
using DataService.Domain.Services.Events;
using DataService.Domain.Tools;
using DataService.Infrastructure.Services;
using DataService.Infrastructure.Services.Events;

namespace DataService.Infrastructure;

public static class ApplicationServicesExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
    {
        @this.AddSingleton<InputValidator>();
        @this.AddSingleton<SwissKnife>();

        // add mappers
        @this.AddSingleton<EntityMapper>();
        @this.AddSingleton<TenantMapper>();

        // add dapper and database connection factory
        @this.AddScoped<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // add repositories       
        DiscoverAndRegisterRepositories(services: @this);

        // add services
        @this.AddScoped<ITenantService, TenantService>();

        // add pulbisher and consumers for events
        @this.AddSingleton<IEventPublisher, EventBrokerAdapter>();
        @this.AddSingleton<IEventConsumer, EventBrokerAdapter>();

        return @this;
    }

    /// <summary>
    /// Discovers and registers all repository implementations that implement the IRepository interface in the current assembly.
    /// </summary>
    /// <param name="services"></param>
    private static void DiscoverAndRegisterRepositories(IServiceCollection services)
    {
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
        // Find all interfaces that implement IRepository
        var baseRepositoryInterfaceType = typeof(IRepository);
        var respositoryInterfaces = types
            .Where(type => baseRepositoryInterfaceType.IsAssignableFrom(type) && type.IsInterface && !(type == baseRepositoryInterfaceType));
        foreach (var repositoryInterface in respositoryInterfaces)
        {
            var implementationType = types
                .Where(type => repositoryInterface.IsAssignableFrom(type) && type.IsClass)
                .FirstOrDefault();
            if (implementationType != null)
            {
                services.AddScoped(repositoryInterface, implementationType);
            }
        }
    }   

}
