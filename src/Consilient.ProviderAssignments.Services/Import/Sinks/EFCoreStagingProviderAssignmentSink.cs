using System.Text.Json;
using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Contracts;
using EFCore.BulkExtensions;

namespace Consilient.ProviderAssignments.Services.Import.Sinks
{
    internal class EFCoreStagingProviderAssignmentSink(ConsilientDbContext dbContext) : IDataSink
    {
        private readonly ConsilientDbContext _dbContext = dbContext;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public async Task WriteBatchAsync<TRow>(Guid batchId, IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
            where TRow : class
        {
            if (batch.Count == 0)
            {
                return;
            }

            // Map from ValidatedRow<ProcessedProviderAssignment> to ProviderAssignment entity
            if (batch is IReadOnlyList<ValidatedRow<ProcessedProviderAssignment>> validatedRows)
            {
                var stagingRecords = validatedRows
                    .Select(vr => new ProviderAssignment
                    {
                        BatchId = batchId,
                        // Raw data from vr.Row.Raw
                        Age = vr.Row.Raw.Age,
                        AttendingMD = vr.Row.Raw.AttendingMD ?? string.Empty,
                        HospitalNumber = vr.Row.Raw.HospitalNumber ?? string.Empty,
                        Admit = vr.Row.Raw.Admit,
                        Dob = vr.Row.Raw.Dob,
                        Mrn = vr.Row.Raw.Mrn ?? string.Empty,
                        Name = vr.Row.Raw.Name ?? string.Empty,
                        Insurance = vr.Row.Raw.Insurance ?? string.Empty,
                        NursePractitioner = vr.Row.Raw.NursePractitioner ?? string.Empty,
                        IsCleared = vr.Row.Raw.IsCleared ?? string.Empty,
                        Location = vr.Row.Raw.Location ?? string.Empty,
                        H_P = vr.Row.Raw.H_P ?? string.Empty,
                        PsychEval = vr.Row.Raw.PsychEval ?? string.Empty,
                        // Processed data from vr.Row
                        FacilityId = vr.Row.FacilityId,
                        ServiceDate = vr.Row.ServiceDate,
                        Bed = vr.Row.Bed,
                        Room = vr.Row.Room,
                        NormalizedNursePractitionerLastName = vr.Row.NormalizedNursePractitionerLastName,
                        NormalizedPatientFirstName = vr.Row.NormalizedPatientFirstName,
                        NormalizedPatientLastName = vr.Row.NormalizedPatientLastName,
                        NormalizedPhysicianLastName = vr.Row.NormalizedPhysicianLastName,
                        // Validation state from wrapper
                        ValidationErrorsJson = vr.Errors.Count > 0
                            ? JsonSerializer.Serialize(vr.Errors)
                            : null,
                        ShouldImport = vr.IsValid
                    })
                    .ToList();

                // Use BulkInsertAsync for optimal performance
                await _dbContext.BulkInsertAsync(stagingRecords, cancellationToken: cancellationToken);
            }
            // Backward compatibility: support old ExternalProviderAssignment type
#pragma warning disable CS0618 // Type or member is obsolete
            else if (batch is IReadOnlyList<ExternalProviderAssignment> externalAssignments)
#pragma warning restore CS0618
            {
                var stagingRecords = externalAssignments
                    .Select(ea => new ProviderAssignment
                    {
                        BatchId = batchId,
                        Age = ea.Age,
                        AttendingMD = ea.AttendingMD ?? string.Empty,
                        HospitalNumber = ea.HospitalNumber ?? string.Empty,
                        Admit = ea.Admit,
                        Dob = ea.Dob,
                        FacilityId = ea.FacilityId,
                        Mrn = ea.Mrn ?? string.Empty,
                        Name = ea.Name ?? string.Empty,
                        Insurance = ea.Insurance ?? string.Empty,
                        NursePractitioner = ea.NursePractitioner ?? string.Empty,
                        IsCleared = ea.IsCleared ?? string.Empty,
                        Location = ea.Location ?? string.Empty,
                        ServiceDate = ea.ServiceDate,
                        H_P = ea.H_P ?? string.Empty,
                        PsychEval = ea.PsychEval ?? string.Empty,
                        ValidationErrorsJson = ea.ValidationErrors.Count > 0
                            ? JsonSerializer.Serialize(ea.ValidationErrors)
                            : null,
                        Bed = ea.Bed,
                        Room = ea.Room,
                        NormalizedNursePractitionerLastName = ea.NormalizedNursePractitionerLastName,
                        NormalizedPatientFirstName = ea.NormalizedPatientFirstName,
                        NormalizedPatientLastName = ea.NormalizedPatientLastName,
                        NormalizedPhysicianLastName = ea.NormalizedPhysicianLastName,
                        ShouldImport = true
                    })
                    .ToList();

                await _dbContext.BulkInsertAsync(stagingRecords, cancellationToken: cancellationToken);
            }
            else
            {
#pragma warning disable CS0618 // Type or member is obsolete
                throw new InvalidOperationException(
                    $"Expected batch of type {nameof(ValidatedRow<ProcessedProviderAssignment>)} or {nameof(ExternalProviderAssignment)}, got {typeof(TRow).Name}");
#pragma warning restore CS0618
            }
        }

        public Task FinalizeAsync(CancellationToken cancellationToken = default)
        {
            // No cleanup needed for EF Core
            return Task.CompletedTask;
        }
    }
}
