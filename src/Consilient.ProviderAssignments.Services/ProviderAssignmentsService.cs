using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.Infrastructure.Storage;
using Consilient.Infrastructure.Storage.Contracts;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Consilient.ProviderAssignments.Services;

/// <summary>
/// Service for managing provider assignment imports.
/// Handles file storage, batch creation, and job queue coordination.
/// </summary>
public class ProviderAssignmentsService(
        IBackgroundJobClient backgroundJobClient,
        IFileStorage fileStorage,
        ConsilientDbContext dbContext,
        ILogger<ProviderAssignmentsService> logger) : IProviderAssignmentsService
    {
        public async Task<ImportProviderAssignmentResult> ImportAsync(
            ImportProviderAssignmentRequest request,
            CancellationToken cancellationToken = default)
        {
            var batchId = Guid.NewGuid();

            try
            {
                // 1. Save the file to storage
                var filename = PathHelper.GenerateFileReference(batchId.ToString(), request.File.FileName);
                await using var stream = request.File.OpenReadStream();
                var fileReference = await fileStorage.SaveAsync(filename, stream, cancellationToken).ConfigureAwait(false);

                logger.LogInformation(
                    "File saved to storage for batch {BatchId}. File: {FileName}, Reference: {FileReference}",
                    batchId, request.File.FileName, fileReference);

                // 2. Create the ProviderAssignmentBatch record
                var batch = new ProviderAssignmentBatch
                {
                    Date = request.ServiceDate,
                    FacilityId = request.FacilityId,
                    Status = ProviderAssignmentBatchStatus.Pending,
                    CreatedByUserId = request.EnqueuedByUserId
                };

                // Set the Id using reflection since it has a private setter
                typeof(ProviderAssignmentBatch).BaseType!
                    .GetProperty(nameof(batch.Id))!
                    .SetValue(batch, batchId);

                dbContext.StagingProviderAssignmentBatches.Add(batch);
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                logger.LogInformation(
                    "Batch {BatchId} created with status {Status} for facility {FacilityId} on {ServiceDate} by user {UserId}",
                    batchId, batch.Status, request.FacilityId, request.ServiceDate, request.EnqueuedByUserId);

                // 3. Enqueue the import job
                var importJobId = backgroundJobClient.Enqueue<Background.Workers.ProviderAssignments.ProviderAssignmentsImportWorker>(
                    worker => worker.Import(batchId, fileReference, null!));

                // Chain the resolution job to run after import completes
                backgroundJobClient.ContinueJobWith<Background.Workers.ProviderAssignments.ProviderAssignmentsResolutionWorker>(
                    importJobId,
                    x => x.Resolve(batchId),
                    JobContinuationOptions.OnlyOnSucceededState);

                logger.LogInformation(
                    "Jobs enqueued for batch {BatchId}. Import job: {ImportJobId}",
                    batchId, importJobId);

                // 4. Return result with success flag
                return new ImportProviderAssignmentResult
                {
                    Success = true,
                    Message = "File uploaded successfully and queued for processing.",
                    BatchId = batchId
                };
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Database error while creating batch {BatchId}", batchId);
                return new ImportProviderAssignmentResult
                {
                    Success = false,
                    Message = $"Failed to create import batch: {ex.Message}",
                    BatchId = batchId
                };
            }
            catch (IOException ex)
            {
                logger.LogError(ex, "File storage error for batch {BatchId}", batchId);
                return new ImportProviderAssignmentResult
                {
                    Success = false,
                    Message = $"Failed to save file: {ex.Message}",
                    BatchId = batchId
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error during import for batch {BatchId}", batchId);
            
                // Attempt to clean up batch if it was created
                try
                {
                    var existingBatch = await dbContext.StagingProviderAssignmentBatches
                        .FirstOrDefaultAsync(b => b.Id == batchId, cancellationToken);
                
                    if (existingBatch != null)
                    {
                        dbContext.StagingProviderAssignmentBatches.Remove(existingBatch);
                        await dbContext.SaveChangesAsync(cancellationToken);
                        logger.LogInformation("Cleaned up batch {BatchId} after error", batchId);
                    }
                }
                catch (Exception cleanupEx)
                {
                    logger.LogWarning(cleanupEx, "Failed to clean up batch {BatchId} after error", batchId);
                }

                return new ImportProviderAssignmentResult
                {
                    Success = false,
                    Message = $"Failed to process import: {ex.Message}",
                    BatchId = batchId
                };
            }
        }

        public Task<string> ProcessAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            try
            {
                var jobId = backgroundJobClient.Enqueue<Background.Workers.ProviderAssignments.ProviderAssignmentsProcessWorker>(
                    worker => worker.Process(batchId));

                logger.LogInformation("Process job {JobId} enqueued for batch {BatchId}", jobId, batchId);
            
                return Task.FromResult(jobId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to enqueue process job for batch {BatchId}", batchId);
                throw new InvalidOperationException($"Failed to enqueue processing job: {ex.Message}", ex);
            }
        }

        public Task<IEnumerable<ProviderAssignmentBatchStatusDto>> GetBatchStatusesAsync(CancellationToken cancellationToken = default)
        {
            logger.LogDebug("Retrieving all batch statuses");

            var statuses = Enum.GetValues<ProviderAssignmentBatchStatus>()
                .Select(status => new ProviderAssignmentBatchStatusDto(
                    Value: (int)status,
                    Name: status.ToString()))
                .AsEnumerable();

        return Task.FromResult(statuses);
    }
}
