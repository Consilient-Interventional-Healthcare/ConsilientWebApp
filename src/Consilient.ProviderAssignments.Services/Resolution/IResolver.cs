using Consilient.Data.Entities.Staging;

namespace Consilient.ProviderAssignments.Services.Resolution
{
    /// <summary>
    /// Non-generic base interface for resolver enumeration.
    /// </summary>
    internal interface IResolver
    {
        /// <summary>
        /// Resolves staging records by matching them to existing database entities.
        /// </summary>
        /// <param name="facilityId">The facility ID for the resolution context.</param>
        /// <param name="date">The service date for the resolution context.</param>
        /// <param name="records">The staging records to resolve.</param>
        Task ResolveAsync(int facilityId, DateOnly date, List<ProviderAssignment> records);
    }

    /// <summary>
    /// Generic interface for resolvers that work with a specific entity type.
    /// </summary>
    internal interface IResolver<TEntity> : IResolver where TEntity : class
    {
    }

    // Marker interfaces for each resolver type
    internal interface IPhysicianResolver : IResolver { }
    internal interface INursePractitionerResolver : IResolver { }
    internal interface IPatientResolver : IResolver { }
    internal interface IHospitalizationResolver : IResolver { }
    internal interface IHospitalizationStatusResolver : IResolver { }
    internal interface IVisitResolver : IResolver { }
}