using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Patients.Services
{
    internal class PatientVisitService(ConsilientDbContext dataContext) : IPatientVisitService
    {
        public async Task<PatientVisitDto?> CreateAsync(CreatePatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<Visit>();
            dataContext.PatientVisits.Add(entity);
            await dataContext.SaveChangesAsync();

            return entity.Adapt<PatientVisitDto>();
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

        public async Task<IEnumerable<PatientVisitDto>> GetByDateAsync(DateOnly date)
        {
            var patientVisits = await dataContext.PatientVisits
                .AsNoTracking()
                .Where(e => e.DateServiced == date)
                .ProjectToType<PatientVisitDto>()
                .ToListAsync();
            return patientVisits;
        }

        public async Task<IEnumerable<PatientVisitDto>> GetByEmployeeAsync(int employeeId)
        {
            var patientVisits = await dataContext.PatientVisits
                    .AsNoTracking()
                    .Where(e => e.PhysicianEmployeeId == employeeId || e.NursePractitionerEmployeeId == employeeId)
                    .ProjectToType<PatientVisitDto>()
                    .ToListAsync();
            return patientVisits;
        }

        public async Task<PatientVisitDto?> GetByIdAsync(int id)
        {
            var patientVisit = await dataContext.PatientVisits.FindAsync(id);
            return patientVisit?.Adapt<PatientVisitDto>();
        }

        public async Task<PatientVisitDto?> UpdateAsync(int id, UpdatePatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await dataContext.PatientVisits
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.CosigningPhysicianEmployeeId, _ => request.CosigningPhysicianEmployeeId)
                    //.SetProperty(e => e.FacilityId, _ => request.FacilityId)
                    .SetProperty(e => e.InsuranceId, _ => request.InsuranceId)
                    .SetProperty(e => e.IsScribeServiceOnly, _ => request.IsScribeServiceOnly)
                    .SetProperty(e => e.NursePractitionerEmployeeId, _ => request.NursePractitionerEmployeeId)
                    .SetProperty(e => e.PhysicianEmployeeId, _ => request.PhysicianEmployeeId)
                    .SetProperty(e => e.ScribeEmployeeId, _ => request.ScribeEmployeeId)
                    .SetProperty(e => e.ServiceTypeId, _ => request.ServiceTypeId)
                );

            if (affected == 0)
            {
                return null;
            }

            return await dataContext.PatientVisits
                .AsNoTracking()
                .Where(e => e.Id == id)
                .ProjectToType<PatientVisitDto>()
                .FirstOrDefaultAsync();
        }
    }
}
