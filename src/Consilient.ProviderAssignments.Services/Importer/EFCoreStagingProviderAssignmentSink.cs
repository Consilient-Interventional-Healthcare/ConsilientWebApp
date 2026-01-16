using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using EFCore.BulkExtensions;

namespace Consilient.ProviderAssignments.Services.Importer
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

            // Map from ExternalProviderAssignment to StagingProviderAssignment
            if (batch is IReadOnlyList<ExternalProviderAssignment> externalAssignments)
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
                        ValidationErrorsJson = null
                    })
                    .ToList();

                // Use BulkInsertAsync for optimal performance
                await _dbContext.BulkInsertAsync(stagingRecords, cancellationToken: cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"Expected batch of type {nameof(ExternalProviderAssignment)}, got {typeof(TRow).Name}");
            }
        }

        public Task FinalizeAsync(CancellationToken cancellationToken = default)
        {
            // No cleanup needed for EF Core
            return Task.CompletedTask;
        }
    }
}
