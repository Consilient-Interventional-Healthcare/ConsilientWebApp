using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;

namespace Consilient.Patients.Contracts
{
    public interface IPatientService
    {
        Task<PatientDto?> GetByMrnAsync(int mrn);
        Task<PatientDto> CreateAsync(CreatePatientRequest request);
    }
}
