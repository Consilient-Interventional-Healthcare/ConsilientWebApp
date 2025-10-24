using Consilient.Patients.Contracts.Dtos;

namespace Consilient.Patients.Services.Contracts
{
    public interface IPatientService
    {
        Task<PatientDto?> GetByMrnAsync(int mrn);
    }
}
