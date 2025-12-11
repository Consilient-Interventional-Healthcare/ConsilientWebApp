using Consilient.Background.Workers.Contracts;
using Consilient.Background.Workers.Models;
using Consilient.Infrastructure.ExcelImporter.Core;
using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Sinks;
using Hangfire.Server;
using Microsoft.Extensions.Configuration;

namespace Consilient.Background.Workers
{
    public class ImportDoctorAssignmentsWorker : IBackgroundWorker
    {
        private readonly IExcelImporter<DoctorAssignment> _excelImporter;
        private readonly string _connectionString;

        public ImportDoctorAssignmentsWorker(
            IExcelImporter<DoctorAssignment> excelImporter,
            IConfiguration configuration)
        {
            _excelImporter = excelImporter;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
        }

        // Event for progress reporting using the reusable WorkerProgressEventArgs
        public event EventHandler<WorkerProgressEventArgs>? ProgressChanged;

        public async Task Import(string filePath, DateOnly serviceDate, int facilityId, PerformContext context)
        {
            var jobId = context.BackgroundJob.Id;

            // Configure import options
            var options = new ImportOptions
            {
                Sheet = SheetSelector.FirstSheet,
                ColumnMapping = ColumnMapping.Builder()
                    .MapRequired("Name↓", nameof(DoctorAssignment.Name))
                    .MapRequired("Location", nameof(DoctorAssignment.Location))
                    .MapRequired("Hospital Number", nameof(DoctorAssignment.HospitalNumber))
                    .MapRequired("Admit", nameof(DoctorAssignment.Admit))
                    .MapRequired("MRN", nameof(DoctorAssignment.Mrn))
                    .Map("Age", nameof(DoctorAssignment.Age))
                    .Map("DOB", nameof(DoctorAssignment.Dob))
                    .Map("H&P", nameof(DoctorAssignment.H_P))
                    .Map("Psych Eval", nameof(DoctorAssignment.PsychEval))
                    .Map("Attending MD", nameof(DoctorAssignment.AttendingMD))
                    .Map("Cleared", nameof(DoctorAssignment.IsCleared))
                    .Map("Nurse Practitioner", nameof(DoctorAssignment.NursePractitioner))
                    .Map("Insurance", nameof(DoctorAssignment.Insurance))
                    .Build(),
                BatchSize = 1000,
                FailOnValidationError = false
            };

            // Create destination sink
            var destination = new SqlServerBulkSink(_connectionString, "DoctorAssignmentsStaging");

            // Progress reporting
            var progress = new Progress<ImportProgress>(p =>
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
            });

            try
            {
                // Import using the new pipeline
                var result = await _excelImporter.ImportAsync(
                    filePath,
                    destination,
                    options,
                    progress,
                    CancellationToken.None);

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
