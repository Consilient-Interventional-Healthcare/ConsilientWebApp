using Consilient.Data;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Consilient.Patients.Services
{
    internal class StagingPatientVisitService(ConsilientDbContext dataContext) : IStagingPatientVisitService
    {
        private readonly ConsilientDbContext _dataContext = dataContext;

        public async Task<StagingPatientVisitDto?> CreateAsync(CreateStagingPatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<PatientVisitStaging>();
            _dataContext.PatientVisitsStagings.Add(entity);
            await _dataContext.SaveChangesAsync();

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
                var affected = await _dataContext.PatientVisits
                    .Where(e => e.PatientVisitId == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete employee. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<StagingPatientVisitDto>> GetByDateAsync(DateTime date)
        {
            var stagingPatientVisits = await _dataContext.PatientVisitsStagings
                .AsNoTracking()
                .Where(e => e.DateServiced == DateOnly.FromDateTime(date))
                .ProjectToType<StagingPatientVisitDto>()
                .ToListAsync();
            return stagingPatientVisits;
        }

        public async Task<StagingPatientVisitDto?> GetByIdAsync(int id)
        {
            var stagingPatientVisits = await _dataContext.PatientVisitsStagings.FindAsync(id);
            return stagingPatientVisits?.Adapt<StagingPatientVisitDto>();
        }

        public async Task<StagingPatientVisitDto?> UpdateAsync(int id, UpdateStagingPatientVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await _dataContext.PatientVisitsStagings
                .Where(e => e.PatientVisitStagingId == id)
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

            return await _dataContext.PatientVisitsStagings
                .AsNoTracking()
                .Where(e => e.PatientVisitStagingId == id)
                .ProjectToType<StagingPatientVisitDto>()
                .FirstOrDefaultAsync();
        }
    }
}
