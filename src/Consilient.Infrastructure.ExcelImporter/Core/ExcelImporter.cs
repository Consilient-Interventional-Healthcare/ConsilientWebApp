using Consilient.Infrastructure.ExcelImporter.Exceptions;
using Consilient.Infrastructure.ExcelImporter.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Consilient.Infrastructure.ExcelImporter.Core
{


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

        public async Task<ImportResult> ImportAsync(string sourceFile, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();
            var stats = new ImportStats();
            var validationErrors = new List<ValidationError>();

            try
            {
                logger.LogInformation("Starting import from {SourceFile}", sourceFile);
                OnProgressChanged(new ImportProgressEventArgs { Stage = "Initializing" });
                await destination.InitializeAsync(cancellationToken);

                OnProgressChanged(new ImportProgressEventArgs { Stage = "Reading", CurrentOperation = "Opening file" });
                await using var fileStream = File.OpenRead(sourceFile);

                var batch = new List<TRow>(options.BatchSize);

                await foreach (var excelRow in reader.ReadRowsAsync(fileStream, options.Sheet, cancellationToken))
                {
                    stats.TotalRowsRead++;

                    if (stats.TotalRowsRead > options.MaxRows)
                    {
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

                        if (options.FailOnValidationError)
                        {
                            logger.LogError("Mapping failed at row {RowNumber}: {Error}", excelRow.RowNumber, mappingResult.Error);
                            throw new ImportValidationException(validationErrors);
                        }

                        continue;
                    }

                    var row = mappingResult.Value!;

                    // Validate
                    var isValid = true;
                    foreach (var validator in validators)
                    {
                        var validationResult = validator.Validate(row, excelRow.RowNumber);
                        if (!validationResult.IsValid)
                        {
                            validationErrors.AddRange(validationResult.Errors);
                            isValid = false;

                            if (options.FailOnValidationError)
                            {
                                logger.LogError("Validation failed at row {RowNumber}", excelRow.RowNumber);
                                throw new ImportValidationException(validationErrors);
                            }
                        }
                    }

                    if (!isValid)
                    {
                        stats.TotalRowsSkipped++;
                        continue;
                    }

                    // Transform
                    foreach (var transformer in transformers)
                    {
                        row = transformer.Transform(row);
                    }

                    batch.Add(row);

                    // Write batch when full
                    if (batch.Count >= options.BatchSize)
                    {
                        await destination.WriteBatchAsync(batch, cancellationToken);
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
                    await destination.WriteBatchAsync(batch, cancellationToken);
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
                    Duration = stopwatch.Elapsed,
                    ValidationErrors = validationErrors
                };

                logger.LogInformation(
                    "Import completed: {RowsRead} read, {RowsWritten} written, {RowsSkipped} skipped in {Duration}",
                    result.TotalRowsRead,
                    result.TotalRowsWritten,
                    result.TotalRowsSkipped,
                    result.Duration);

                return result;
            }
            catch (Exception ex) when (ex is not ImportValidationException)
            {
                logger.LogError(ex, "Import failed for file {FileName}", sourceFile);
                throw;
            }
        }

        private class ImportStats
        {
            public int TotalRowsRead { get; set; }
            public int TotalRowsWritten { get; set; }
            public int TotalRowsSkipped { get; set; }
        }
    }
}
