using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.DoctorAssignments.Contracts;
using Consilient.DoctorAssignments.Services.Cache;
using Consilient.DoctorAssignments.Services.Validators;
using Microsoft.EntityFrameworkCore;

namespace Consilient.DoctorAssignments.Services
{
    internal class DoctorAssignmentsResolver(ConsilientDbContext dbContext) : IDoctorAssignmentsResolver
    {
        private readonly ConsilientDbContext _dbContext = dbContext;
        private readonly DoctorAssignmentValidator _validator = new();
        private readonly DoctorAssignmentResolutionCache _cache = new(dbContext);

        public async Task ResolveAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                // Step 1: Read from staging table the batch matching the batchId
                var recordsList = await GetStagingRecordsForResolution(batchId, cancellationToken);

                if (recordsList.Count == 0)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return;
                }

                // Step 2: Validate data integrity, annotate errors if any
                ValidateDataIntegrity(recordsList, _cache);

                // Step 4: Resolve attendingPhysicianId via attendingMD
                ResolveAttendingPhysicians(recordsList);

                // Step 5: Resolve nursePractitionerId via nursePractitioner
                ResolveNursePractitioners(recordsList);

                // Step 6 & 7: Apply resolution rules and mark exclusions
                ApplyResolutionRules(recordsList);

                // Step 8: Resolve patientId via Mrn
                ResolvePatients(recordsList);

                // Step 9: Resolve hospitalizationId via HospitalizationNumber
                ResolveHospitalizations(recordsList);

                // Step 10: Update staging table with validation errors in bulk
                await BulkUpdateAllChanges(recordsList);

                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private async Task<List<Data.Entities.DoctorAssignment>> GetStagingRecordsForResolution(Guid batchId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<DoctorAssignment>()
                .Where(x => x.BatchId == batchId
                    && (x.ValidationErrors == null || x.ValidationErrors.Count == 0)
                    && !x.ShouldImport
                    && x.ResolvedVisitId == null
                    && !x.Imported)
                .ToListAsync(cancellationToken);
        }

        private void ValidateDataIntegrity(List<DoctorAssignment> records, DoctorAssignmentResolutionCache cache)
        {
            // Run validator on each record and add validation errors to the record
            foreach (var record in records)
            {
                var validationErrors = _validator.Validate(record);

                // Additional validation: Check if FacilityId exists in the database
                if (record.FacilityId.HasValue && !cache.FacilityIds.Contains(record.FacilityId.Value))
                {
                    validationErrors.Add($"Facility ID {record.FacilityId} does not exist");
                }

                if (validationErrors.Count > 0)
                {
                    record.ValidationErrors.AddRange(validationErrors);
                }
            }
        }

        private async Task BulkUpdateAllChanges(List<DoctorAssignment> records)
        {
            var recordIds = records.Select(r => r.Id).ToList();

            foreach (var record in records)
            {
                _dbContext.Set<Data.Entities.DoctorAssignment>().Attach(record);
                _dbContext.Entry(record).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
        }

        private void ResolveAttendingPhysicians(List<DoctorAssignment> records)
        {
            ResolveProviders(records,
                record => record.AttendingMD,
                (record, id) => record.ResolvedProviderId = id,
                "Attending physician");
        }

        private void ResolveNursePractitioners(List<DoctorAssignment> records)
        {
            ResolveProviders(records,
                record => record.NursePractitioner,
                (record, id) => record.ResolvedNursePracticionerId = id,
                "Nurse practitioner");
        }

        /// <summary>
        /// Resolves provider information with consistent error handling.
        /// </summary>
        private void ResolveProviders(
            List<DoctorAssignment> records,
            Func<DoctorAssignment, string?> nameSelector,
            Action<DoctorAssignment, int> idSetter,
            string providerType)
        {
            foreach (var record in records)
            {
                if (record.ValidationErrors.Count > 0 || string.IsNullOrWhiteSpace(nameSelector(record)))
                {
                    continue;
                }

                var providerName = nameSelector(record)!.Trim();
                var matchedEmployeeId = FindEmployeeByName(providerName);

                if (matchedEmployeeId.HasValue)
                {
                    idSetter(record, matchedEmployeeId.Value);
                }
                else
                {
                    record.ValidationErrors.Add($"{providerType} not found");
                }
            }
        }

        private int? FindEmployeeByName(string fullName)
        {
            // Try exact match with just first and last name
            if (_cache.EmployeeIdsByFullName.TryGetValue(fullName, out var exactMatches))
            {
                // Return first match if providers only
                foreach (var employeeId in exactMatches)
                {
                    if (_cache.EmployeeDetailsById[employeeId].IsProvider)
                    {
                        return employeeId;
                    }
                }
            }

            // Try match with title extension (space-separated)
            if (_cache.EmployeeIdsByFullNameWithTitle.TryGetValue(fullName, out var withTitleMatches))
            {
                foreach (var employeeId in withTitleMatches)
                {
                    if (_cache.EmployeeDetailsById[employeeId].IsProvider)
                    {
                        return employeeId;
                    }
                }
            }

            // Try match with title extension (comma-separated)
            if (_cache.EmployeeIdsByFullNameWithTitleComma.TryGetValue(fullName, out var withTitleCommaMatches))
            {
                foreach (var employeeId in withTitleCommaMatches)
                {
                    if (_cache.EmployeeDetailsById[employeeId].IsProvider)
                    {
                        return employeeId;
                    }
                }
            }

            return null;
        }

        private static void ApplyResolutionRules(List<DoctorAssignment> records)
        {
            foreach (var record in records)
            {
                if (record.ValidationErrors.Count > 0 || record.ResolvedProviderId == null)
                {
                    continue;
                }

                // Mark for import if we have a resolved provider
                record.ShouldImport = true;
            }
        }

        private void ResolvePatients(List<DoctorAssignment> records)
        {
            foreach (var record in records)
            {
                if (record.ValidationErrors.Count > 0 || !record.ShouldImport || string.IsNullOrWhiteSpace(record.Mrn) || !record.FacilityId.HasValue)
                {
                    continue;
                }

                var key = (record.Mrn, record.FacilityId.Value);
                if (_cache.PatientIdByMrnAndFacility.TryGetValue(key, out var patientId))
                {
                    record.ResolvedPatientId = patientId;
                }
                else
                {
                    record.NeedsNewPatient = true;
                }
            }
        }

        private void ResolveHospitalizations(List<DoctorAssignment> records)
        {
            foreach (var record in records)
            {
                if (record.ValidationErrors.Count > 0 || !record.ShouldImport || string.IsNullOrWhiteSpace(record.HospitalNumber) || !record.FacilityId.HasValue || !record.ResolvedPatientId.HasValue)
                {
                    continue;
                }

                if (int.TryParse(record.HospitalNumber, out var caseId))
                {
                    var key = (caseId, record.FacilityId.Value);
                    if (_cache.HospitalizationsByCaseIdAndFacility.TryGetValue(key, out var hospitalizations))
                    {
                        // Find matching hospitalization with all three criteria: CaseId, FacilityId, and PatientId
                        var matching = hospitalizations.FirstOrDefault(h =>
                            h.patientId == record.ResolvedPatientId.Value);

                        if (matching != default)
                        {
                            record.ResolvedHospitalizationId = matching.hospitalizationId;
                        }
                        else
                        {
                            record.NeedsNewHospitalization = true;
                        }
                    }
                    else
                    {
                        record.NeedsNewHospitalization = true;
                    }
                }
            }
        }
    }
}
