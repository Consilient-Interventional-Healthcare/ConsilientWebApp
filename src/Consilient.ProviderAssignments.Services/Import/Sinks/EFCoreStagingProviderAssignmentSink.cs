using Consilient.Data;
using Consilient.Data.Entities.Staging;
using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Consilient.ProviderAssignments.Services.Import.Sinks;

/// <summary>
/// Writes provider assignment data to the staging tables in the database.
/// Each WriteBatchAsync call persists the batch record, items, and updates status atomically.
/// </summary>
internal class EFCoreStagingProviderAssignmentSink(
    ConsilientDbContext dbContext,
    ILogger<EFCoreStagingProviderAssignmentSink> logger) : IDataSink
{
    public Task InitializeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public async Task WriteBatchAsync<TRow>(Guid batchId, IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
        where TRow : class
    {
        if (batch.Count == 0)
        {
            return;
        }

        if (batch is not IReadOnlyList<ValidatedRow<ProcessedProviderAssignment>> validatedRows)
        {
            throw new InvalidOperationException(
                $"Expected {nameof(ValidatedRow<ProcessedProviderAssignment>)}, got {typeof(TRow).Name}");
        }

        var facilityId = validatedRows[0].Row.FacilityId;
        var serviceDate = validatedRows[0].Row.ServiceDate;

        logger.LogInformation(
            "Persisting batch {BatchId} for facility {FacilityId} on {ServiceDate} with {Count} records",
            batchId, facilityId, serviceDate, validatedRows.Count);

        var records = validatedRows.Select(vr => MapToEntity(batchId, vr)).ToList();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create batch record
                var batchRecord = new ProviderAssignmentBatch
                {
                    Date = serviceDate,
                    FacilityId = facilityId,
                    Status = ProviderAssignmentBatchStatus.Pending
                };
                dbContext.StagingProviderAssignmentBatches.Add(batchRecord);
                dbContext.Entry(batchRecord).Property(e => e.Id).CurrentValue = batchId;
                await dbContext.SaveChangesAsync(cancellationToken);

                // Insert all records using standard EF Core
                dbContext.StagingProviderAssignments.AddRange(records);
                await dbContext.SaveChangesAsync(cancellationToken);

                // Update batch status
                batchRecord.Status = ProviderAssignmentBatchStatus.Imported;
                await dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                logger.LogInformation("Batch {BatchId} committed successfully", batchId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to persist batch {BatchId}, rolling back", batchId);
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    public Task FinalizeAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    // SQL Server smalldatetime minimum value (1900-01-01)
    private static readonly DateTime SqlSmallDateTimeMin = new(1900, 1, 1);

    private static ProviderAssignment MapToEntity(Guid batchId, ValidatedRow<ProcessedProviderAssignment> vr)
    {
        // Coerce invalid DateTime values to SQL-compatible minimum
        var admit = vr.Row.Raw.Admit;
        if (admit < SqlSmallDateTimeMin)
        {
            admit = SqlSmallDateTimeMin;
        }

        return new ProviderAssignment
        {
            BatchId = batchId,
            // Raw data
            Age = vr.Row.Raw.Age,
            AttendingMD = vr.Row.Raw.AttendingMD ?? string.Empty,
            HospitalNumber = vr.Row.Raw.HospitalNumber ?? string.Empty,
            Admit = admit,
            Dob = vr.Row.Raw.Dob,
            Mrn = vr.Row.Raw.Mrn ?? string.Empty,
            Name = vr.Row.Raw.Name ?? string.Empty,
            Insurance = vr.Row.Raw.Insurance ?? string.Empty,
            NursePractitioner = vr.Row.Raw.NursePractitioner ?? string.Empty,
            IsCleared = vr.Row.Raw.IsCleared ?? string.Empty,
            Location = vr.Row.Raw.Location ?? string.Empty,
            H_P = vr.Row.Raw.H_P ?? string.Empty,
            PsychEval = vr.Row.Raw.PsychEval ?? string.Empty,
            // Processed data
            FacilityId = vr.Row.FacilityId,
            ServiceDate = vr.Row.ServiceDate,
            Bed = vr.Row.Bed,
            Room = vr.Row.Room,
            NormalizedNursePractitionerLastName = vr.Row.NormalizedNursePractitionerLastName,
            NormalizedPatientFirstName = vr.Row.NormalizedPatientFirstName,
            NormalizedPatientLastName = vr.Row.NormalizedPatientLastName,
            NormalizedPhysicianLastName = vr.Row.NormalizedPhysicianLastName,
            // Validation
            ValidationErrorsJson = vr.Errors.Count > 0 ? JsonSerializer.Serialize(vr.Errors) : null,
            ShouldImport = vr.IsValid
        };
    }
}
