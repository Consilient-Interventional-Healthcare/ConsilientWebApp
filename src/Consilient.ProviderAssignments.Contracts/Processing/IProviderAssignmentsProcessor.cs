namespace Consilient.ProviderAssignments.Contracts.Processing
{
    /// <summary>
    /// Processes resolved provider assignments from the staging table into production tables.
    /// Executes the final phase of the import pipeline via stored procedure.
    /// </summary>
    public interface IProviderAssignmentsProcessor
    {
        /// <summary>
        /// Processes a batch of resolved provider assignments.
        /// Creates missing patients, providers, hospitalizations, and visits as needed.
        /// </summary>
        /// <param name="batchId">The batch ID to process.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Result containing processed count and any errors.</returns>
        Task<ProcessResult> ProcessAsync(Guid batchId, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Result of processing a batch of provider assignments.
    /// </summary>
    /// <param name="ProcessedCount">Number of records successfully processed.</param>
    /// <param name="ErrorCount">Number of records that failed processing.</param>
    /// <param name="ErrorMessage">Error message if processing failed, null otherwise.</param>
    public record ProcessResult(int ProcessedCount, int ErrorCount, string? ErrorMessage);
}
