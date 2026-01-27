using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Common.Contracts;
using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.Infrastructure.Storage.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Consilient.Background.Workers.ProviderAssignments;

public class ProviderAssignmentsImportWorker(
    IImporterFactory importerFactory,
    IUserContextSetter userContextSetter,
    IFileStorage fileStorage,
    ConsilientDbContext dbContext,
    ILogger<ProviderAssignmentsImportWorker> logger) : IBackgroundWorker
    {
        // Event for progress reporting using the reusable WorkerProgressEventArgs
        public event EventHandler<WorkerProgressEventArgs>? ProgressChanged;

    [DisplayName("Import Provider Assignments (Batch: {0})")]
    public async Task<Guid> Import(Guid batchId, string fileReference, PerformContext context)
    {
        var jobId = context.BackgroundJob.Id;

        try
        {
            // Retrieve the batch to get all necessary details
            var batch = await dbContext.StagingProviderAssignmentBatches
                .FirstOrDefaultAsync(b => b.Id == batchId);

            if (batch == null)
            {
                logger.LogError("Batch {BatchId} not found in database", batchId);
                throw new InvalidOperationException($"Batch {batchId} does not exist. Import cannot proceed.");
            }

            if (batch.Status != ProviderAssignmentBatchStatus.Pending)
            {
                logger.LogWarning(
                    "Batch {BatchId} is in status {Status}, expected Pending. Skipping import.",
                    batchId, batch.Status);
                return batchId;
            }

            // Set the user context from the batch
            userContextSetter.SetUser(batch.CreatedByUserId);

            logger.LogInformation(
                "Starting import for batch {BatchId}, facility {FacilityId}, service date {ServiceDate}",
                batchId, batch.FacilityId, batch.Date);

        // Create importer using factory (use batch details)
        var importer = importerFactory.Create(batch.FacilityId, batch.Date);

        // Wire up progress events
        importer.ProgressChanged += (sender, p) =>
        {
            var workerEvent = new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = p.Stage,
                    TotalItems = p.TotalRows ?? 0,
                    ProcessedItems = p.ProcessedRows,
                    CurrentOperation = p.CurrentOperation,
                    Timestamp = DateTime.UtcNow
                };

                OnProgressChanged(workerEvent);
            };

            // Get file stream from storage
            await using var fileStream = await fileStorage.GetAsync(fileReference, CancellationToken.None);

            // Import using the stream-based pipeline
            var result = await importer.ImportAsync(batchId, fileStream, CancellationToken.None);

            // Update batch status to Imported
            batch.Status = ProviderAssignmentBatchStatus.Imported;
            await dbContext.SaveChangesAsync();

            logger.LogInformation(
                "Import completed for batch {BatchId}. Rows read: {RowsRead}, Rows written: {RowsWritten}, Status updated to {Status}",
                batchId, result.TotalRowsRead, result.TotalRowsWritten, batch.Status);

                // Report completion
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = "Completed",
                    CurrentOperation = $"Import completed: {result.TotalRowsWritten} rows imported",
                    Timestamp = DateTime.UtcNow,
                    TotalItems = result.TotalRowsRead,
                    ProcessedItems = result.TotalRowsWritten,
                AdditionalData = new Dictionary<string, object>
                {
                    ["FileReference"] = fileReference,
                    ["ServiceDate"] = batch.Date.ToString("yyyy-MM-dd"),
                    ["FacilityId"] = batch.FacilityId,
                    ["BatchId"] = batchId.ToString(),
                    ["TotalRowsRead"] = result.TotalRowsRead,
                    ["TotalRowsWritten"] = result.TotalRowsWritten,
                    ["TotalRowsSkipped"] = result.TotalRowsSkipped,
                    ["Duration"] = result.Duration.ToString(),
                    ["ValidationErrors"] = result.ValidationErrors.Count
                }
            });

            return batchId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Import failed for batch {BatchId}", batchId);

            // Forward failure event
                OnProgressChanged(new WorkerProgressEventArgs
                {
                    JobId = jobId,
                    Stage = "Failed",
                    CurrentOperation = $"Import failed: {ex.Message}",
                    Timestamp = DateTime.UtcNow,
                AdditionalData = new Dictionary<string, object>
                {
                    ["ErrorMessage"] = ex.Message,
                    ["ErrorType"] = ex.GetType().Name,
                    ["FileReference"] = fileReference
                }
                });

            throw;
        }
    }

    protected virtual void OnProgressChanged(WorkerProgressEventArgs e)
    {
        ProgressChanged?.Invoke(this, e);
    }
}
