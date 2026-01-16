using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Common.Services;
using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.Storage.Contracts;
using Hangfire;
using Hangfire.Server;

namespace Consilient.Background.Workers.ProviderAssignments
{
    public class ProviderAssignmentsImportWorker(
        IImporterFactory importerFactory,
        IUserContextSetter userContextSetter,
        IFileStorage fileStorage) : IBackgroundWorker
    {
        // Event for progress reporting using the reusable WorkerProgressEventArgs
        public event EventHandler<WorkerProgressEventArgs>? ProgressChanged;

        public async Task<Guid> Import(string fileReference, int facilityId, DateOnly serviceDate, int enqueuedByUserId, PerformContext context)
        {
            // Set the user context for this job scope
            userContextSetter.SetUser(enqueuedByUserId);

            var jobId = context.BackgroundJob.Id;

            // Create importer using factory
            var importer = importerFactory.Create(facilityId, serviceDate);

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

            try
            {
                // Get file stream from storage
                await using var fileStream = await fileStorage.GetAsync(fileReference, CancellationToken.None);

                // Import using the stream-based pipeline
                var result = await importer.ImportAsync(fileStream, CancellationToken.None);

                var batchId = result.BatchId ?? throw new InvalidOperationException("BatchId was not generated during import");

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
                        ["ServiceDate"] = serviceDate.ToString("yyyy-MM-dd"),
                        ["FacilityId"] = facilityId,
                        ["BatchId"] = batchId.ToString(),
                        ["TotalRowsRead"] = result.TotalRowsRead,
                        ["TotalRowsWritten"] = result.TotalRowsWritten,
                        ["TotalRowsSkipped"] = result.TotalRowsSkipped,
                        ["Duration"] = result.Duration.ToString(),
                        ["ValidationErrors"] = result.ValidationErrors.Count
                    }
                });

                // Optionally clean up the file after successful import
                await fileStorage.DeleteAsync(fileReference, CancellationToken.None);

                return batchId;
            }
            catch (Exception ex)
            {
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
}
