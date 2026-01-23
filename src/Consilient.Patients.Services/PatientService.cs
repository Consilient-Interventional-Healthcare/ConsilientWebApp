using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Models;
using Consilient.Patients.Contracts.Models.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Patients.Services
{
    public class PatientService(ConsilientDbContext dataContext) : IPatientService
    {
        public async Task<PatientDto?> CreateAsync(CreatePatientRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            if (request.FacilityId.HasValue && !string.IsNullOrEmpty(request.Mrn))
            {
                var exists = await dataContext.Set<PatientFacility>().AnyAsync(e => e.Mrn.ToString() == request.Mrn && e.FacilityId == request.FacilityId);
                if (exists)
                {
                    throw new InvalidOperationException("A patient with the specified MRN already exists.");
                }
            }
            var entity = request.Adapt<Patient>();
            dataContext.Patients.Add(entity);
            await dataContext.SaveChangesAsync();
            return entity.Adapt<PatientDto>();
        }

        public Task<PatientDto?> GetByMrnAsync(int mrn)
        {
            //var patient = await dataContext.Patients.FirstOrDefaultAsync(p => p.Mrn == mrn);
            //return patient?.Adapt<PatientDto>();
            throw new NotImplementedException();
        }
    }
}
