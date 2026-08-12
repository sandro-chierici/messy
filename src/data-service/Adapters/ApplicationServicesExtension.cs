using DataService.Adapters.Repository;
using DataService.Business.IO;
using DataService.Business.IO.Mapper;
using DataService.Business.Repository;
using DataService.Business.Rules;
using DataService.Business.Tools;

namespace DataService.Adapters
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
        {
            @this.AddSingleton<InputValidator>();
            @this.AddSingleton<EntityMapper>();
            @this.AddSingleton<SwissKnife>();

            // add dapper and database connection factory
            @this.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

            // add repositories
            @this.AddSingleton<ITenantRepository, TenantRepository>();

            return @this;
        }
    }
}
