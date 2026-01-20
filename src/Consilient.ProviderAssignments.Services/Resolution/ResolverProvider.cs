using Consilient.Data;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Services.Resolution.Resolvers;
using Microsoft.Extensions.DependencyInjection;

namespace Consilient.ProviderAssignments.Services.Resolution
{
    /// <summary>
    /// Creates resolver instances using the DI container.
    /// Resolvers are registered with their marker interfaces and instantiated
    /// with explicit cache and dbContext parameters shared per resolution cycle.
    /// </summary>
    internal class ResolverProvider(IServiceProvider serviceProvider) : IResolverProvider
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public IEnumerable<TResolverType> GetResolvers<TResolverType>(IResolutionCache cache, ConsilientDbContext dbContext) where TResolverType : IResolver
        {
            // Get all registered resolver types for this marker interface
            var resolverTypes = _serviceProvider.GetServices<TResolverType>();

            foreach (var resolverType in resolverTypes)
            {
                // Use ActivatorUtilities to create instance with both DI and explicit parameters
                // DI resolves: ILogger<T>
                // Explicit parameters: cache, dbContext
                var resolver = (TResolverType)ActivatorUtilities.CreateInstance(
                    _serviceProvider,
                    resolverType.GetType(),
                    cache,
                    dbContext);

                yield return resolver;
            }
        }
    }
}
