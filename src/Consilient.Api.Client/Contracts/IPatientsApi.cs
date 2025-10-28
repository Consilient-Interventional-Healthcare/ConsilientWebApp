using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IPatientsApi : IApi
    {
        Task<ApiResponse<IEnumerable<PatientDto>>> GetAllAsync();
        Task<ApiResponse<PatientDto?>> CreateAsync(CreatePatientRequest request);
        Task<ApiResponse<PatientDto?>> GetByMrnAsync(int mrn);
    }
}
