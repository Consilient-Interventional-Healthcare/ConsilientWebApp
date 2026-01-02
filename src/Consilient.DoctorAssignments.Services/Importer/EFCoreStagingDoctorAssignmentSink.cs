using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.DoctorAssignments.Contracts;
using Consilient.Infrastructure.ExcelImporter.Core;
using EFCore.BulkExtensions;

namespace Consilient.DoctorAssignments.Services.Importer
{
    internal class EFCoreStagingDoctorAssignmentSink(ConsilientDbContext dbContext) : IDataSink
    {
        private readonly ConsilientDbContext _dbContext = dbContext;

        public Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            // EF Core doesn't require special initialization
            return Task.CompletedTask;
        }

        public async Task<Guid?> WriteBatchAsync<TRow>(IReadOnlyList<TRow> batch, CancellationToken cancellationToken = default)
            where TRow : class
        {
            if (batch.Count == 0)
            {
                return null;
            }

            // Map from ExternalDoctorAssignment to StagingDoctorAssignment
            if (batch is IReadOnlyList<ExternalDoctorAssignment> externalAssignments)
            {
                var batchId = Guid.NewGuid();
                var stagingRecords = externalAssignments
                    .Select(ea => new DoctorAssignment
                    {
                        BatchId = batchId,
                        Age = ea.Age,
                        AttendingMD = ea.AttendingMD,
                        HospitalNumber = ea.HospitalNumber,
                        Admit = ea.Admit,
                        Dob = ea.Dob,
                        FacilityId = ea.FacilityId,
                        Mrn = ea.Mrn,
                        Name = ea.Name,
                        Insurance = ea.Insurance,
                        NursePractitioner = ea.NursePractitioner,
                        IsCleared = ea.IsCleared,
                        Location = ea.Location,
                        ServiceDate = ea.ServiceDate,
                        H_P = ea.H_P,
                        PsychEval = ea.PsychEval,
                        ValidationErrors = []
                    })
                    .ToList();

                // Use BulkInsertAsync for optimal performance
                await _dbContext.BulkInsertAsync(stagingRecords, cancellationToken: cancellationToken);
                return batchId;
            }
            else
            {
                throw new InvalidOperationException($"Expected batch of type {nameof(ExternalDoctorAssignment)}, got {typeof(TRow).Name}");
            }
        }

        public Task FinalizeAsync(CancellationToken cancellationToken = default)
        {
            // No cleanup needed for EF Core
            return Task.CompletedTask;
        }
    }
}
