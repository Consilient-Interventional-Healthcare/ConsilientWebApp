using Consilient.Patients.Contracts.Dtos;

namespace Consilient.Patients.Contracts
{
    public interface IPatientService
    {
        Task<PatientDto?> GetByMrnAsync(int mrn);
    }
}
