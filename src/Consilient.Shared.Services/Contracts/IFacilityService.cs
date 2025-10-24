using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;

namespace Consilient.Shared.Services.Contracts
{
    public interface IFacilityService
    {
        Task<IEnumerable<FacilityDto>> GetAllAsync();
        Task<FacilityDto?> GetById(int id);
        Task<FacilityDto> CreateAsync(CreateFacilityRequest request);
        Task<FacilityDto?> UpdateAsync(int id, UpdateFacilityRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
