namespace Consilient.ProviderAssignments.Contracts;

/// <summary>
/// Service for managing provider assignments operations.
/// </summary>
public interface IProviderAssignmentsService
{
    /// <summary>
    /// Imports provider assignments from an uploaded file.
    /// </summary>
    /// <param name="request">The import request containing file and metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing batch information and status message.</returns>
    Task<ImportProviderAssignmentResult> ImportAsync(ImportProviderAssignmentRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enqueues a processing job for a specific batch.
    /// </summary>
    /// <param name="batchId">The batch ID to process.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Hangfire job ID.</returns>
    Task<string> ProcessAsync(Guid batchId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available batch statuses.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of batch status DTOs.</returns>
    Task<IEnumerable<ProviderAssignmentBatchStatusDto>> GetBatchStatusesAsync(CancellationToken cancellationToken = default);
}