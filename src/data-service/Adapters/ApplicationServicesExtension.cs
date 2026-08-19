using DataService.Adapters.Repository;
using DataService.Business.IO;
using DataService.Business.IO.Mapper;
using DataService.Business.Repository;
using DataService.Business.Rules;
using DataService.Business.Tools;

namespace DataService.Adapters;

public static class ApplicationServicesExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
    {
        @this.AddSingleton<InputValidator>();
        @this.AddSingleton<SwissKnife>();
        @this.AddSingleton<EntityMapper>();;

        // add dapper and database connection factory
        @this.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // add repositories       
        DiscoverAndRegisterRepositories(services: @this);

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
