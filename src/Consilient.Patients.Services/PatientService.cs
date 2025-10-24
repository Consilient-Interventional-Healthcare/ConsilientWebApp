using Consilient.Data;
using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Services.Contracts;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Patients.Services
{
    public class PatientService(ConsilientDbContext dataContext) : IPatientService
    {
        private readonly ConsilientDbContext dataContext = dataContext;

        public async Task<PatientDto?> GetByMrnAsync(int mrn)
        {
            var patient = await dataContext.Patients.FirstOrDefaultAsync(p => p.PatientMrn == mrn);
            return patient?.Adapt<PatientDto>();
        }
    }
}
