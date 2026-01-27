using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Exceptions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Consilient.Infrastructure.ExcelImporter;



public class ExcelImporter<TRow>(
    IExcelReader reader,
    IRowMapper<TRow> mapper,
    IEnumerable<IRowValidator<TRow>> validators,
    IEnumerable<IRowTransformer<TRow>> transformers,
    IDataSink destination,
    ImportOptions options,
    ILogger<ExcelImporter<TRow>> logger) : IExcelImporter<TRow> where TRow : class
{
    public event EventHandler<ImportProgressEventArgs>? ProgressChanged;

    protected virtual void OnProgressChanged(ImportProgressEventArgs progress)
    {
        ProgressChanged?.Invoke(this, progress);
    }

    public async Task<ImportResult> ImportAsync(Guid batchId, Stream stream, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var stats = new ImportStats();
        var validationErrors = new List<ValidationError>();

        try
        {
            OnProgressChanged(new ImportProgressEventArgs { Stage = "Initializing" });
            await destination.InitializeAsync(cancellationToken);

            OnProgressChanged(new ImportProgressEventArgs { Stage = "Reading", CurrentOperation = "Opening file" });

            var batch = new List<TRow>(options.BatchSize);

            await foreach (var excelRow in reader.ReadRowsAsync(stream, options.Sheet, cancellationToken))
            {
                stats.TotalRowsRead++;

                if (stats.TotalRowsRead > options.MaxRows)
                {
                    break;
                }

                // Check if we should stop reading based on custom predicate
                if (options.ShouldStopReading?.Invoke(excelRow, options.ColumnMapping) == true)
                {
                    logger.LogInformation(
                        "Stopping import at row {RowNumber}: ShouldStopReading predicate returned true",
                        excelRow.RowNumber);
                    break;
                }

                // Map
                var mappingResult = mapper.Map(excelRow, options.ColumnMapping);
                if (!mappingResult.IsSuccess)
                {
                    stats.TotalRowsSkipped++;
                    validationErrors.Add(new ValidationError(
                        excelRow.RowNumber,
                        "",
                        mappingResult.Error!,
                        ValidationSeverity.Error));

                    logger.Log(options.FailOnValidationError ? LogLevel.Error : LogLevel.Warning, "Mapping failed at row {RowNumber}: {Error}", excelRow.RowNumber, mappingResult.Error);

                    if (options.FailOnValidationError)
                    {
                        throw new ImportValidationException(validationErrors);
                    }

                    continue;
                }

                var row = mappingResult.Value!;

                // Validate
                var rowErrors = new List<string>();
                foreach (var validator in validators)
                {
                    var validationResult = validator.Validate(row, excelRow.RowNumber);
                    if (!validationResult.IsValid)
                    {
                        validationErrors.AddRange(validationResult.Errors);
                        rowErrors.AddRange(validationResult.Errors.Select(e => e.Message));

                        if (options.FailOnValidationError)
                        {
                            logger.LogError("Validation failed at row {RowNumber}", excelRow.RowNumber);
                            throw new ImportValidationException(validationErrors);
                        }

                        logger.LogWarning("Validation failed at row {RowNumber}: {Errors}",
                            excelRow.RowNumber, string.Join(", ", rowErrors));
                    }
                }
                // Transform (all rows, including invalid ones)
                foreach (var transformer in transformers)
                {
                    row = transformer.Transform(row);
                }

                batch.Add(row);

                // Write batch when full
                if (batch.Count >= options.BatchSize)
                {
                    await destination.WriteBatchAsync(batchId, batch, cancellationToken);
                    stats.TotalRowsWritten += batch.Count;
                    batch.Clear();

                    OnProgressChanged(new ImportProgressEventArgs
                    {
                        Stage = "Processing",
                        ProcessedRows = stats.TotalRowsWritten,
                        CurrentOperation = $"Written {stats.TotalRowsWritten} rows"
                    });
                }
            }

            // Always report Processing stage, even if no batches were written yet
            if (stats.TotalRowsWritten == 0)
            {
                OnProgressChanged(new ImportProgressEventArgs { Stage = "Processing", CurrentOperation = "Processing data" });
            }

            // Write remaining
            if (batch.Count > 0)
            {
                await destination.WriteBatchAsync(batchId, batch, cancellationToken);
                stats.TotalRowsWritten += batch.Count;

                OnProgressChanged(new ImportProgressEventArgs
                {
                    Stage = "Processing",
                    ProcessedRows = stats.TotalRowsWritten,
                    CurrentOperation = $"Written {stats.TotalRowsWritten} rows"
                });
            }

            OnProgressChanged(new ImportProgressEventArgs { Stage = "Finalizing", ProcessedRows = stats.TotalRowsWritten });
            await destination.FinalizeAsync(cancellationToken);

            stopwatch.Stop();

            var result = new ImportResult
            {
                TotalRowsRead = stats.TotalRowsRead,
                TotalRowsWritten = stats.TotalRowsWritten,
                TotalRowsSkipped = stats.TotalRowsSkipped,
                TotalRowsWithErrors = stats.TotalRowsWithErrors,
                Duration = stopwatch.Elapsed,
                ValidationErrors = validationErrors,
                BatchId = batchId
            };

            logger.LogInformation(
                "Import completed: {RowsRead} read, {RowsWritten} written, {RowsSkipped} skipped, {RowsWithErrors} with validation errors in {Duration}",
                result.TotalRowsRead,
                result.TotalRowsWritten,
                result.TotalRowsSkipped,
                result.TotalRowsWithErrors,
                result.Duration);

            return result;
        }
        catch (Exception ex) when (ex is not ImportValidationException)
        {
            logger.LogError(ex, "Import failed");
            throw;
        }
    }

    private class ImportStats
    {
        public int TotalRowsRead { get; set; }
        public int TotalRowsWritten { get; set; }
        public int TotalRowsSkipped { get; set; }
        public int TotalRowsWithErrors { get; set; }
    }
}
