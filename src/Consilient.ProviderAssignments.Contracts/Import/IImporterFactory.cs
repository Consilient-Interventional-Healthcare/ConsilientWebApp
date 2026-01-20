namespace Consilient.ProviderAssignments.Contracts.Import
{
    /// <summary>
    /// Factory for creating provider assignment importers configured for a specific facility and service date.
    /// </summary>
    public interface IImporterFactory
    {
        /// <summary>
        /// Creates a new importer instance configured for the specified facility and service date.
        /// </summary>
        /// <param name="facilityId">The facility ID for the import context.</param>
        /// <param name="serviceDate">The service date for the imported assignments.</param>
        /// <returns>A configured importer ready to process Excel files.</returns>
        IProviderAssignmentsImporter Create(int facilityId, DateOnly serviceDate);
    }
}
