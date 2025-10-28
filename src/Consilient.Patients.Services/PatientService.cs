using Consilient.Data;
using Consilient.Patients.Contracts;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Patients.Services
{
    public class PatientService(ConsilientDbContext dataContext) : IPatientService
    {
        private readonly ConsilientDbContext _dataContext = dataContext;

        public async Task<PatientDto> CreateAsync(CreatePatientRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            var exists = await _dataContext.Patients.AnyAsync(e => e.PatientMrn == request.PatientMrn);
            if (exists)
            {
                throw new InvalidOperationException("A patient with the specified MRN already exists.");
            }
            var entity = request.Adapt<Patient>();
            _dataContext.Patients.Add(entity);
            await _dataContext.SaveChangesAsync();

            return entity.Adapt<PatientDto>();
        }

        public async Task<PatientDto?> GetByMrnAsync(int mrn)
        {
            var patient = await _dataContext.Patients.FirstOrDefaultAsync(p => p.PatientMrn == mrn);
            return patient?.Adapt<PatientDto>();
        }
    }
}
