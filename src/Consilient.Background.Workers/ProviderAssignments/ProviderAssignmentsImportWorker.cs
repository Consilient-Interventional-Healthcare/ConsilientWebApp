using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Common.Services;
using Consilient.Infrastructure.Storage.Contracts;
using Hangfire;
using Hangfire.Server;
using System.ComponentModel;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.Background.Workers.ProviderAssignments
{
    public class ProviderAssignmentsImportWorker(
        IImporterFactory importerFactory,
        IUserContextSetter userContextSetter,
        IFileStorage fileStorage) : IBackgroundWorker
    {
        // Event for progress reporting using the reusable WorkerProgressEventArgs
        public event EventHandler<WorkerProgressEventArgs>? ProgressChanged;

        [DisplayName("Import Provider Assignments (Batch: {0})")]
        public async Task<Guid> Import(Guid batchId, ProviderAssignmentsImportInput input, PerformContext context)
        {
            // Set the user context for this job scope
            userContextSetter.SetUser(input.EnqueuedByUserId);

            var jobId = context.BackgroundJob.Id;

            // Create importer using factory
            var importer = importerFactory.Create(input.FacilityId, input.ServiceDate);

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
                await using var fileStream = await fileStorage.GetAsync(input.FileReference, CancellationToken.None);

                // Import using the stream-based pipeline
                var result = await importer.ImportAsync(batchId, fileStream, CancellationToken.None);

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
                        ["FileReference"] = input.FileReference,
                        ["ServiceDate"] = input.ServiceDate.ToString("yyyy-MM-dd"),
                        ["FacilityId"] = input.FacilityId,
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
                        ["FileReference"] = input.FileReference
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
