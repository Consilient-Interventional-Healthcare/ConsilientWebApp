using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IServiceTypesApi : IApi
    {
        Task<ApiResponse<ServiceTypeDto?>> CreateAsync(CreateServiceTypeRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<IEnumerable<ServiceTypeDto>>> GetAllAsync();
        Task<ApiResponse<ServiceTypeDto?>> GetByIdAsync(int id);
        Task<ApiResponse<ServiceTypeDto?>> UpdateAsync(int id, UpdateServiceTypeRequest request);
    }
}