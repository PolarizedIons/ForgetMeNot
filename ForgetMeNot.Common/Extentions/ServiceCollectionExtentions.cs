using System.Data;
using Microsoft.Extensions.DependencyInjection;

namespace ForgetMeNot.Common.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection DiscoverDiServices(this IServiceCollection services)
        {
            var discoveredTypes = typeof(IDiService).GetAllInAssembly();
            foreach (var serviceType in discoveredTypes)
            {
                if (typeof(IScopedDiService).IsAssignableFrom(serviceType))
                {
                    services.AddScoped(serviceType);
                }
                else if (typeof(ISingletonDiService).IsAssignableFrom(serviceType))
                {
                    services.AddSingleton(serviceType);
                }
                else
                {
                    throw new InvalidConstraintException("Unknown type of DI Service found! " + serviceType); 
                }
            }

            return services;
        }
    }

    public interface IDiService {}
    public interface IScopedDiService : IDiService {}
    public interface ISingletonDiService : IDiService {}
}
