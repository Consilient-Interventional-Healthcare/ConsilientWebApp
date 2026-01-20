namespace Consilient.ProviderAssignments.Contracts.Resolution
{
    /// <summary>
    /// Resolves provider assignment records by matching staging data to existing database entities.
    /// </summary>
    /// <remarks>
    /// Resolution workflow:
    /// <list type="number">
    ///   <item>Load staging records for the batch (excludes records with validation errors)</item>
    ///   <item>Create a shared resolution cache for the cycle</item>
    ///   <item>Run resolvers in dependency order:
    ///     <list type="bullet">
    ///       <item>Physician - matches by normalized last name</item>
    ///       <item>Nurse Practitioner - matches by normalized last name</item>
    ///       <item>Patient - matches by MRN and facility</item>
    ///       <item>Hospitalization - matches by case ID, patient, and facility</item>
    ///       <item>Visit - matches by hospitalization and service date</item>
    ///     </list>
    ///   </item>
    ///   <item>Bulk update staging table with resolved IDs</item>
    /// </list>
    ///
    /// Note: Patient/provider creation and eligibility filtering are handled by the
    /// processing phase (stored procedure), not the resolution phase.
    /// </remarks>
    public interface IProviderAssignmentsResolver
    {
        /// <summary>
        /// Resolves staging records by matching them to existing database entities.
        /// </summary>
        /// <param name="batchId">The batch ID to resolve.</param>
        /// <param name="facilityId">The facility ID for the import.</param>
        /// <param name="date">The service date for the import.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="progress">Optional progress reporter for tracking resolution stages.</param>
        Task ResolveAsync(
            Guid batchId,
            int facilityId,
            DateOnly date,
            CancellationToken cancellationToken = default,
            IProgress<ResolutionProgressEventArgs>? progress = null);
    }
}
