using Consilient.ProviderAssignments.Contracts.Validation;

namespace Consilient.ProviderAssignments.Contracts.Resolution;

/// <summary>
/// Non-generic base interface for resolver enumeration.
/// </summary>
public interface IResolver
{
    /// <summary>
    /// Resolves staging records by matching them to existing database entities.
    /// </summary>
    /// <param name="facilityId">The facility ID for the resolution context.</param>
    /// <param name="date">The service date for the resolution context.</param>
    /// <param name="contexts">The validation contexts wrapping staging records to resolve.</param>
    Task ResolveAsync(int facilityId, DateOnly date, List<RowValidationContext> contexts);
}

/// <summary>
/// Generic interface for resolvers that work with a specific entity type.
/// </summary>
public interface IResolver<TEntity> : IResolver where TEntity : class
{
}

// Marker interfaces for each resolver type
public interface IPhysicianResolver : IResolver { }
public interface INursePractitionerResolver : IResolver { }
public interface IPatientResolver : IResolver { }
public interface IHospitalizationResolver : IResolver { }
public interface IHospitalizationStatusResolver : IResolver { }
public interface IVisitResolver : IResolver { }