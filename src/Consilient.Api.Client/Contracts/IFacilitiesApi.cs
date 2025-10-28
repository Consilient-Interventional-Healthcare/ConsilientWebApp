using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IFacilitiesApi : IApi
    {
        Task<ApiResponse<IEnumerable<FacilityDto>>> GetAllAsync();
        Task<ApiResponse<FacilityDto?>> GetByIdAsync(int id);
        Task<ApiResponse<FacilityDto?>> CreateAsync(CreateFacilityRequest request);
        Task<ApiResponse<FacilityDto?>> UpdateAsync(int id, UpdateFacilityRequest request);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}