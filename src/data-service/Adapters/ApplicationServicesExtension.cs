using DataService.Business.Rules;
using System.Runtime.CompilerServices;

namespace DataService.Adapters
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection @this)
        {
            @this.AddSingleton<InputValidator>();

            return @this;
        }
    }
}
