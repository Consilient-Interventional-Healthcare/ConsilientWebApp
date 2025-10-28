using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IInsurancesApi : IApi
    {
        Task<ApiResponse<IEnumerable<InsuranceDto>>> GetAllAsync();
        Task<ApiResponse<InsuranceDto?>> GetByIdAsync(int id);
        Task<ApiResponse<InsuranceDto>> CreateAsync(CreateInsuranceRequest request);
        Task<ApiResponse<InsuranceDto>> UpdateAsync(int id, UpdateInsuranceRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}
