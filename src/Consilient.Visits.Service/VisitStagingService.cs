using ClosedXML.Excel;
using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Dtos;
using Consilient.Visits.Contracts.Requests;
using Consilient.Visits.Contracts.Results;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Visits.Services
{
    internal class VisitStagingService(ConsilientDbContext dataContext) : IVisitStagingService
    {
        public async Task<VisitStagingDto?> CreateAsync(CreateVisitStagingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<VisitStaging>();
            dataContext.VisitsStaging.Add(entity);
            await dataContext.SaveChangesAsync();

            return entity.Adapt<VisitStagingDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            try
            {
                var affected = await dataContext.Visits
                    .Where(e => e.Id == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete employee. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<VisitStagingDto>> GetByDateAsync(DateOnly date)
        {
            var visitsStaging = await dataContext.VisitsStaging
                    .AsNoTracking()
                    .Where(e => e.DateServiced == date)
                    .ProjectToType<VisitStagingDto>()
                    .ToListAsync();
            return visitsStaging;
        }

        public async Task<IEnumerable<VisitStagingDto>> GetByEmployeeAsync(int employeeId)
        {
            var visitsStaging = await dataContext.VisitsStaging
                    .AsNoTracking()
                    .Where(e => e.PhysicianEmployeeId == employeeId || e.NursePractitionerEmployeeId == employeeId)
                    .ProjectToType<VisitStagingDto>()
                    .ToListAsync();
            return visitsStaging;
        }

        public async Task<VisitStagingDto?> GetByIdAsync(int id)
        {
            var visitsStaging = await dataContext.VisitsStaging.FindAsync(id);
            return visitsStaging?.Adapt<VisitStagingDto>();
        }

        public async Task<int> PushApprovedVisitsAsync()
        {
            var approvedVisitsStaging = await dataContext.VisitsStaging
                .Where(p => p.PhysicianApproved && !p.AddedToMainTable
                            && (p.NursePractitionerEmployeeId == null || p.NursePractitionerApproved))
                .ToListAsync();
            if (approvedVisitsStaging.Count == 0)
            {
                return approvedVisitsStaging.Count;
            }
            foreach (var VisitStaging in approvedVisitsStaging)
            {
                var Visit = VisitStaging.Adapt<Visit>();
                dataContext.Update(Visit);

                VisitStaging.AddedToMainTable = true; // Mark as added to main table
                dataContext.Update(VisitStaging);
            }
            await dataContext.SaveChangesAsync();
            return approvedVisitsStaging.Count;
        }

        public async Task<VisitStagingDto?> UpdateAsync(int id, UpdateVisitStagingRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await dataContext.VisitsStaging
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

            return await dataContext.VisitsStaging
                .AsNoTracking()
                .Where(e => e.Id == id)
                .ProjectToType<VisitStagingDto>()
                .FirstOrDefaultAsync();
        }
        public async Task<UploadAssignmentResult> UploadAssignmentAsync(MemoryStream spreadsheet)
        {
            var records = new List<VisitStaging>();
            using (var stream = new MemoryStream())
            {
                await spreadsheet.CopyToAsync(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                {
                    return new UploadAssignmentResult(false, "The spreadsheet is empty or not formatted correctly.");
                }

                var rows = worksheet.RowsUsed().Skip(8); // Skip header rows

                foreach (var row in rows)
                {
                    var facilityId = dataContext.Facilities
                            .Where(f => f.Name == "Santa Rosa Hospital")
                            .Select(f => f.Id)
                            .FirstOrDefault();

                    if (facilityId == 0)
                    {
                        return new UploadAssignmentResult(false, "Facility 'Santa Rosa Hospital' not found. Please add it via the Administration menu.");
                    }

                    var serviceTypeId = dataContext.ServiceTypes
                        .Where(s => s.Cptcode == Convert.ToInt32(row.Cell(4).GetString().Trim())
                                    && s.Description == row.Cell(5).GetString().Trim())
                        .Select(s => s.Id)
                        .FirstOrDefault();

                    if (serviceTypeId == 0)
                    {
                        return new UploadAssignmentResult(false, $"Service type with CPT code {row.Cell(4).GetString().Trim()} and description {row.Cell(5).GetString().Trim()} not found. Please correct on spreadsheet or add service type to records via the Administration menu.");
                    }

                    var physicianEmployeeId = dataContext.Employees
                            .Where(e => e.LastName == row.Cell(6).GetString().Trim())
                            .Select(e => e.Id)
                            .FirstOrDefault();

                    if (physicianEmployeeId == 0)
                    {
                        return new UploadAssignmentResult(false, $"Physician {row.Cell(6).GetString().Trim()} not found. Please correct name on spreadsheet or add employee to records via the Administration menu.");
                    }

                    var nursePractitionerCell = row.Cell(7).GetString().Trim();

                    var nursePractitionerEmployeeId = dataContext.Employees
                            .Where(e => e.LastName == nursePractitionerCell)
                            .Select(e => e.Id)
                            .FirstOrDefault();

                    if (!string.IsNullOrEmpty(nursePractitionerCell) && nursePractitionerEmployeeId == 0)
                    {
                        return new UploadAssignmentResult(false, $"Nurse Practitioner {nursePractitionerCell} not found.\r\n");
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


                    var VisitStaging = new VisitStaging
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
                    records.Add(VisitStaging);
                }
            }
            dataContext.VisitsStaging.AddRange(records);
            await dataContext.SaveChangesAsync();

            return new UploadAssignmentResult(true, $"{records.Count} patient visits have been added to staging.");
        }

    }
}
