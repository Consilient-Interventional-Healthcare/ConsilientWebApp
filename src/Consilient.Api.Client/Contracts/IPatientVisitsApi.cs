using Consilient.Patients.Contracts.Dtos;
using Consilient.Patients.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IPatientVisitsApi<TDto> : IApi where TDto : class
    {
        Task<ApiResponse<TDto?>> CreateAsync(CreatePatientVisitRequest request);

        Task<ApiResponse<bool>> DeleteAsync(int id);

        Task<ApiResponse<IEnumerable<TDto>>> GetByDateAsync(DateTime date);

        Task<ApiResponse<TDto?>> GetByIdAsync(int id);
        Task<ApiResponse<TDto?>> UpdateAsync(int id, UpdatePatientVisitRequest request);
    }

    public interface IPatientVisitsApi : IPatientVisitsApi<PatientVisitDto>
    {
    }
}
