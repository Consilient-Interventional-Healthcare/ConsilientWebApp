using Consilient.Data;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Patients.Services
{
    internal class PatientVisitService(ConsilientDbContext dataContext) : IPatientVisitService
    {
        private readonly ConsilientDbContext _dataContext = dataContext;

        public async Task<PatientVisitDto?> CreateAsync(CreatePatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<PatientVisit>();
            _dataContext.PatientVisits.Add(entity);
            await _dataContext.SaveChangesAsync();

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
                var affected = await _dataContext.PatientVisitsStagings
                    .Where(e => e.PatientVisitStagingId == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete employee. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<PatientVisitDto>> GetByDateAsync(DateTime date)
        {
            var patientVisits = await _dataContext.PatientVisitsStagings
                .AsNoTracking()
                .Where(e => e.DateServiced == DateOnly.FromDateTime(date))
                .ProjectToType<PatientVisitDto>()
                .ToListAsync();
            return patientVisits;
        }

        public async Task<PatientVisitDto?> GetByIdAsync(int id)
        {
            var patientVisit = await _dataContext.PatientVisits.FindAsync(id);
            return patientVisit?.Adapt<StagingPatientVisitDto>();
        }

        public async Task<PatientVisitDto?> UpdateAsync(int id, UpdatePatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await _dataContext.PatientVisits
                .Where(e => e.PatientVisitId == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.CosigningPhysicianEmployeeId, _ => request.CosigningPhysicianEmployeeId)
                    .SetProperty(e => e.FacilityId, _ => request.FacilityId)
                    .SetProperty(e => e.InsuranceId, _ => request.InsuranceId)
                    .SetProperty(e => e.IsScribeServiceOnly, _ => request.IsScribeServiceOnly)
                    .SetProperty(e => e.IsSupervising, _ => request.IsSupervising)
                    .SetProperty(e => e.NursePractitionerEmployeeId, _ => request.NursePractitionerEmployeeId)
                    .SetProperty(e => e.PhysicianEmployeeId, _ => request.PhysicianEmployeeId)
                    .SetProperty(e => e.ScribeEmployeeId, _ => request.ScribeEmployeeId)
                    .SetProperty(e => e.ServiceTypeId, _ => request.ServiceTypeId)
                );

            if (affected == 0)
            {
                return null;
            }

            return await _dataContext.PatientVisits
                .AsNoTracking()
                .Where(e => e.PatientVisitId == id)
                .ProjectToType<PatientVisitDto>()
                .FirstOrDefaultAsync();
        }
    }
}
