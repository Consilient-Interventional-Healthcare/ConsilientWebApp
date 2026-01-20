using Consilient.Data;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Consilient.ProviderAssignments.Services.Resolution.Resolvers;

namespace Consilient.ProviderAssignments.Services.Resolution
{
    /// <summary>
    /// Provides resolver instances for provider assignment resolution.
    /// </summary>
    internal interface IResolverProvider
    {
        /// <summary>
        /// Gets resolvers of the specified type with the given cache and database context.
        /// </summary>
        /// <typeparam name="TResolverType">The resolver marker interface type (e.g., IPatientResolver).</typeparam>
        /// <param name="cache">The shared resolution cache for the resolution cycle.</param>
        /// <param name="dbContext">The database context for the resolution cycle.</param>
        /// <returns>An enumerable of resolvers matching the specified type.</returns>
        IEnumerable<TResolverType> GetResolvers<TResolverType>(IResolutionCache cache, ConsilientDbContext dbContext) where TResolverType : IResolver;
    }
}
