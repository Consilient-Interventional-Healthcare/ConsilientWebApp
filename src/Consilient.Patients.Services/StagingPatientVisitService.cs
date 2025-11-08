using ClosedXML.Excel;
using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Consilient.Patients.Contracts.Results;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Patients.Services
{
    internal class StagingPatientVisitService(ConsilientDbContext dataContext) : IStagingPatientVisitService
    {
        public async Task<StagingPatientVisitDto?> CreateAsync(CreateStagingPatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<PatientVisitStaging>();
            dataContext.PatientVisitsStagings.Add(entity);
            await dataContext.SaveChangesAsync();

            return entity.Adapt<StagingPatientVisitDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            try
            {
                var affected = await dataContext.PatientVisits
                    .Where(e => e.Id == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete employee. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<StagingPatientVisitDto>> GetByDateAsync(DateOnly date)
        {
            var stagingPatientVisits = await dataContext.PatientVisitsStagings
                    .AsNoTracking()
                    .Where(e => e.DateServiced == date)
                    .ProjectToType<StagingPatientVisitDto>()
                    .ToListAsync();
            return stagingPatientVisits;
        }

        public async Task<IEnumerable<StagingPatientVisitDto>> GetByEmployeeAsync(int employeeId)
        {
            var stagingPatientVisits = await dataContext.PatientVisitsStagings
                    .AsNoTracking()
                    .Where(e => e.PhysicianEmployeeId == employeeId || e.NursePractitionerEmployeeId == employeeId)
                    .ProjectToType<StagingPatientVisitDto>()
                    .ToListAsync();
            return stagingPatientVisits;
        }

        public async Task<StagingPatientVisitDto?> GetByIdAsync(int id)
        {
            var stagingPatientVisits = await dataContext.PatientVisitsStagings.FindAsync(id);
            return stagingPatientVisits?.Adapt<StagingPatientVisitDto>();
        }

        public async Task<int> PushApprovedPatientVisitsAsync()
        {
            var approvedPatientVisitsStaging = await dataContext.PatientVisitsStagings
                .Where(p => p.PhysicianApproved && !p.AddedToMainTable
                            && (p.NursePractitionerEmployeeId == null || p.NursePractitionerApproved))
                .ToListAsync();
            if (approvedPatientVisitsStaging.Count == 0)
            {
                return approvedPatientVisitsStaging.Count;
            }
            foreach (var patientVisitStaging in approvedPatientVisitsStaging)
            {
                var patientVisit = patientVisitStaging.Adapt<Visit>();
                dataContext.Update(patientVisit);

                patientVisitStaging.AddedToMainTable = true; // Mark as added to main table
                dataContext.Update(patientVisitStaging);
            }
            await dataContext.SaveChangesAsync();
            return approvedPatientVisitsStaging.Count;
        }

        public async Task<StagingPatientVisitDto?> UpdateAsync(int id, UpdateStagingPatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await dataContext.PatientVisitsStagings
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.CosigningPhysicianEmployeeId, _ => request.CosigningPhysicianEmployeeId)
                    .SetProperty(e => e.FacilityId, _ => request.FacilityId)
                    .SetProperty(e => e.InsuranceId, _ => request.InsuranceId)
                    .SetProperty(e => e.IsScribeServiceOnly, _ => request.IsScribeServiceOnly)
                    //.SetProperty(e => e.IsSupervising, _ => request.IsSupervising)
                    .SetProperty(e => e.NursePractitionerEmployeeId, _ => request.NursePractitionerEmployeeId)
                    .SetProperty(e => e.PhysicianEmployeeId, _ => request.PhysicianEmployeeId)
                    .SetProperty(e => e.ScribeEmployeeId, _ => request.ScribeEmployeeId)
                    .SetProperty(e => e.ServiceTypeId, _ => request.ServiceTypeId)
                );

            if (affected == 0)
            {
                return null;
            }

            return await dataContext.PatientVisitsStagings
                .AsNoTracking()
                .Where(e => e.Id == id)
                .ProjectToType<StagingPatientVisitDto>()
                .FirstOrDefaultAsync();
        }
        public async Task<UploadSpreadsheetResult> UploadSpreadsheetAsync(MemoryStream spreadsheet)
        {
            var records = new List<PatientVisitStaging>();
            using (var stream = new MemoryStream())
            {
                await spreadsheet.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return new UploadSpreadsheetResult(false, "The spreadsheet is empty or not formatted correctly.");
                }

                var rows = worksheet.RowsUsed().Skip(8); // Skip header rows

                foreach (var row in rows)
                {
                    var facilityId = dataContext.Facilities
                            .Where(f => f.FacilityName == "Santa Rosa Hospital")
                            .Select(f => f.Id)
                            .FirstOrDefault();

                    if (facilityId == 0)
                    {
                        return new UploadSpreadsheetResult(false, "Facility 'Santa Rosa Hospital' not found. Please add it via the Administration menu.");
                    }

                    var serviceTypeId = dataContext.ServiceTypes
                        .Where(s => s.Cptcode == Convert.ToInt32(row.Cell(4).GetString().Trim())
                                    && s.Description == row.Cell(5).GetString().Trim())
                        .Select(s => s.Id)
                        .FirstOrDefault();

                    if (serviceTypeId == 0)
                    {
                        return new UploadSpreadsheetResult(false, $"Service type with CPT code {row.Cell(4).GetString().Trim()} and description {row.Cell(5).GetString().Trim()} not found. Please correct on spreadsheet or add service type to records via the Administration menu.");
                    }

                    var physicianEmployeeId = dataContext.Employees
                            .Where(e => e.LastName == row.Cell(6).GetString().Trim())
                            .Select(e => e.Id)
                            .FirstOrDefault();

                    if (physicianEmployeeId == 0)
                    {
                        return new UploadSpreadsheetResult(false, $"Physician {row.Cell(6).GetString().Trim()} not found. Please correct name on spreadsheet or add employee to records via the Administration menu.");
                    }

                    var nursePractitionerCell = row.Cell(7).GetString().Trim();

                    var nursePractitionerEmployeeId = dataContext.Employees
                            .Where(e => e.LastName == nursePractitionerCell)
                            .Select(e => e.Id)
                            .FirstOrDefault();

                    if (!string.IsNullOrEmpty(nursePractitionerCell) && nursePractitionerEmployeeId == 0)
                    {
                        return new UploadSpreadsheetResult(false, $"Nurse Practitioner {nursePractitionerCell} not found.\r\n");
                    }

                    var cellH = row.Cell(8).GetString().ToLower();
                    int? scribeEmployeeId = null;
                    if (!string.IsNullOrEmpty(cellH))
                    {
                        var scribeFirstName = cellH.Split(' ')[0].Trim();
                        var scribeLastInitial = cellH.Split(' ')[1].Trim().TrimEnd('.');

                        scribeEmployeeId = dataContext.Employees
                                .Where(e => (e.FirstName ?? string.Empty).Equals(scribeFirstName, StringComparison.CurrentCultureIgnoreCase) && (e.LastName ?? string.Empty).ToLower().Substring(0, 1) == scribeLastInitial)
                                .Select(e => e.Id)
                                .FirstOrDefault();
                    }


                    var patientVisitStaging = new PatientVisitStaging
                    {
                        DateServiced = DateOnly.FromDateTime(row.Cell(1).GetDateTime()),
                        //PatientName = row.Cell(2).GetString(),
                        //PatientMrn = Convert.ToInt32(row.Cell(3).GetString()),
                        FacilityId = facilityId,
                        ServiceTypeId = serviceTypeId,
                        PhysicianEmployeeId = physicianEmployeeId,
                        NursePractitionerEmployeeId = nursePractitionerEmployeeId == 0 ? null : nursePractitionerEmployeeId,
                        ScribeEmployeeId = scribeEmployeeId == 0 ? null : scribeEmployeeId
                    };
                    records.Add(patientVisitStaging);
                }
            }
            dataContext.PatientVisitsStagings.AddRange(records);
            await dataContext.SaveChangesAsync();

            return new UploadSpreadsheetResult(true, $"{records.Count} patient visits have been added to staging.");
        }

    }
}
