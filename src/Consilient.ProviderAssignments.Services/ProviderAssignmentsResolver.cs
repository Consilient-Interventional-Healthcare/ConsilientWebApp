using Consilient.Common;
using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services.Cache;
using Consilient.ProviderAssignments.Services.Validators;
using Microsoft.EntityFrameworkCore;

namespace Consilient.ProviderAssignments.Services
{
    internal class ProviderAssignmentsResolver(ConsilientDbContext dbContext) : IProviderAssignmentsResolver
    {
        private readonly ProviderAssignmentResolutionCache _cache = new(dbContext);
        private readonly ConsilientDbContext _dbContext = dbContext;
        private readonly ProviderAssignmentValidator _validator = new();
        public async Task ResolveAsync(Guid batchId, CancellationToken cancellationToken = default)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
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
            });
        }

        private static void ApplyResolutionRules(List<ProviderAssignment> records)
        {
            foreach (var record in records)
            {
                if (record.HasValidationErrors || record.ResolvedProviderId == null)
                {
                    continue;
                }

                // Mark for import if we have a resolved provider
                record.ShouldImport = true;
            }
        }

        private static List<EmployeeContractRow> FindEmployeesByName(string searchName, List<EmployeeContractRow> candidates)
        {
            var strategies = new[]
            {
                searchName,  // Exact match: "FirstName LastName"
                // Add more name-matching strategies here:
                // Example: searchName with title extensions
                // Example: fuzzy matching variants
            };

            foreach (var strategy in strategies)
            {
                var matches = candidates
                    .Where(e => $"{e.FirstName} {e.LastName}".Equals(strategy, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matches.Count > 0)
                {
                    return matches;
                }
            }

            return [];
        }

        private static bool HasActiveContractOnDateAndFacility(EmployeeContractRow employee, DateOnly visitDate, int facilityId)
        {
            if (employee.ProviderContractId == null)
            {
                return false;
            }

            return employee.StartDate <= visitDate && 
                (employee.EndDate == null || visitDate <= employee.EndDate) && 
                employee.FacilityId == facilityId;
        }

        private async Task BulkUpdateAllChanges(List<ProviderAssignment> records)
        {
            var recordIds = records.Select(r => r.Id).ToList();

            foreach (var record in records)
            {
                _dbContext.StagingProviderAssignments.Attach(record);
                _dbContext.Entry(record).State = EntityState.Modified;
            }

            await _dbContext.SaveChangesAsync();
        }

        private async Task<List<ProviderAssignment>> GetStagingRecordsForResolution(Guid batchId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.StagingProviderAssignments
                .Where(x => x.BatchId == batchId
                    && string.IsNullOrEmpty(x.ValidationErrorsJson)
                    && !x.ShouldImport
                    && x.ResolvedVisitId == null
                    && !x.Imported)
                .ToListAsync(cancellationToken);
        }

        private void ResolveAttendingPhysicians(List<ProviderAssignment> records)
        {
            ResolveProviders(records,
                record => record.AttendingMD,
                (record, id) => record.ResolvedProviderId = id,
                EmployeeRole.Provider);
        }

        private void ResolveHospitalizations(List<ProviderAssignment> records)
        {
            foreach (var record in records)
            {
                if (record.HasValidationErrors || !record.ShouldImport || string.IsNullOrWhiteSpace(record.HospitalNumber) || !record.ResolvedPatientId.HasValue)
                {
                    continue;
                }

                if (int.TryParse(record.HospitalNumber, out var caseId))
                {
                    var key = (caseId, record.FacilityId);
                    if (_cache.Hospitalizations.TryGetValue(key, out var hospitalizations))
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

        private void ResolveNursePractitioners(List<ProviderAssignment> records)
        {
            ResolveProviders(records,
                record => record.NursePractitioner,
                (record, id) => record.ResolvedNursePracticionerId = id,
                EmployeeRole.NursePractitioner);
        }

        private void ResolvePatients(List<ProviderAssignment> records)
        {
            foreach (var record in records)
            {
                if (record.HasValidationErrors || !record.ShouldImport || string.IsNullOrWhiteSpace(record.Mrn))
                {
                    continue;
                }

                var key = (record.Mrn, record.FacilityId);
                if (_cache.Patients.TryGetValue(key, out var patientId))
                {
                    record.ResolvedPatientId = patientId;
                }
                else
                {
                    record.NeedsNewPatient = true;
                }
            }
        }

        private void ResolveProviders(
            List<ProviderAssignment> records,
            Func<ProviderAssignment, string?> nameSelector,
            Action<ProviderAssignment, int> idSetter,
            EmployeeRole role)
        {
            var allEmployees = _cache.Employees;

            // Step 1: Filter by date and facility (for all employees)
            var first = records.First();
            var visitDate = first.ServiceDate;
            var facilityId = first.FacilityId;

            var activeByDateAndFacility = allEmployees
                .Where(e => HasActiveContractOnDateAndFacility(e, visitDate, facilityId))
                .ToList();

            // Step 2: Filter by role
            var activeByDateFacilityAndRole = activeByDateAndFacility
                .Where(e => e.Role == role)
                .ToList();

            // Step 3: Try to match by name using defined strategies
            foreach (var record in records)
            {
                if (record.HasValidationErrors || string.IsNullOrWhiteSpace(nameSelector(record)))
                {
                    continue;
                }

                var providerName = nameSelector(record)!.Trim();

                // Apply name-matching strategies
                var matches = FindEmployeesByName(providerName, activeByDateFacilityAndRole);
                var matchesGroupedById = matches.GroupBy(e => e.Id).Select(g => g.First()).ToList();
                if (matchesGroupedById.Count == 0)
                {
                    record.AddValidationError($"{providerName} not found");
                }
                else if (matchesGroupedById.Count == 1)
                {
                    idSetter(record, matchesGroupedById.First().Id);
                }
                else
                {
                    record.AddValidationError($"Multiple matches found for {providerName}");
                }
            }
        }

        private void ValidateDataIntegrity(List<ProviderAssignment> records, ProviderAssignmentResolutionCache cache)
        {
            // Run validator on each record and add validation errors to the record
            foreach (var record in records)
            {
                var validationErrors = _validator.Validate(record);

                // Additional validation: Check if FacilityId exists in the database
                if (!cache.Facilities.Contains(record.FacilityId))
                {
                    validationErrors.Add($"Facility ID {record.FacilityId} does not exist");
                }

                if (validationErrors.Count > 0)
                {
                    record.AddValidationErrors(validationErrors);
                }
            }
        }
    }
}
