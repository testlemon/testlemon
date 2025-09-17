using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Testlemon.Core.Validators.Abstract;

namespace Testlemon.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        // Auto-discover and register singletons from a specified assembly
        public static IServiceCollection AddAutoDiscoveredValidatorsFromAssembly(this IServiceCollection services, string assemblyPath)
        {
            // Load the external assembly
            var externalAssembly = Assembly.LoadFrom(assemblyPath);

            // Find all types in the external assembly that implement IValidator
            var validators = externalAssembly.GetTypes()
                .Where(type => typeof(IValidator).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            // Register each discovered validator as a singleton
            foreach (var validator in validators)
            {
                services.AddSingleton(typeof(IValidator), validator);
            }

            return services;
        }
    }
}