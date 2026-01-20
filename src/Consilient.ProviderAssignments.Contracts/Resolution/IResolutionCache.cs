namespace Consilient.ProviderAssignments.Contracts.Resolution
{
    /// <summary>
    /// Thread-safe cache for entity lookups during the resolution phase.
    /// Prevents repeated database queries for the same entity types within a resolution cycle.
    /// </summary>
    public interface IResolutionCache
    {
        /// <summary>
        /// Fills the cache with entities if not already populated, using the provided loader function.
        /// </summary>
        /// <typeparam name="T">The entity type to cache.</typeparam>
        /// <param name="loader">Function to load entities if cache is empty.</param>
        /// <returns>The cached collection of entities.</returns>
        IReadOnlyCollection<T> FillCache<T>(Func<IReadOnlyCollection<T>> loader);

        /// <summary>
        /// Gets the cached collection for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type to retrieve.</typeparam>
        /// <returns>The cached collection.</returns>
        /// <exception cref="InvalidOperationException">Thrown if cache has not been filled for this type.</exception>
        IReadOnlyCollection<T> Get<T>();

        /// <summary>
        /// Checks whether the cache has been filled for the specified entity type.
        /// </summary>
        /// <typeparam name="T">The entity type to check.</typeparam>
        /// <returns>True if cache exists for this type, false otherwise.</returns>
        bool HasCache<T>();
    }
}