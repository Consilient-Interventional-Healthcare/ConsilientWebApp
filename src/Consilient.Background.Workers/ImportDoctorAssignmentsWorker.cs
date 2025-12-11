using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Infrastructure.ExcelImporter.Factories;
using Hangfire.Server;

namespace Consilient.Background.Workers
{
    public class ImportDoctorAssignmentsWorker(IImporterFactory importerFactory, string connectionString) : IBackgroundWorker
    {
        // Event for progress reporting using the reusable WorkerProgressEventArgs
        public event EventHandler<WorkerProgressEventArgs>? ProgressChanged;

        public async Task Import(string filePath, DateOnly serviceDate, int facilityId, PerformContext context)
        {
            var jobId = context.BackgroundJob.Id;

            // Create importer using factory
            var importer = importerFactory.Create(connectionString, facilityId, serviceDate);

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
                // Import using the new pipeline
                var result = await importer.ImportAsync(filePath, CancellationToken.None);

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
                        ["FileName"] = Path.GetFileName(filePath),
                        ["ServiceDate"] = serviceDate.ToString("yyyy-MM-dd"),
                        ["FacilityId"] = facilityId,
                        ["BatchId"] = result.BatchId?.ToString() ?? string.Empty,
                        ["TotalRowsRead"] = result.TotalRowsRead,
                        ["TotalRowsWritten"] = result.TotalRowsWritten,
                        ["TotalRowsSkipped"] = result.TotalRowsSkipped,
                        ["Duration"] = result.Duration.ToString(),
                        ["ValidationErrors"] = result.ValidationErrors.Count
                    }
                });
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
                        ["FileName"] = Path.GetFileName(filePath)
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
