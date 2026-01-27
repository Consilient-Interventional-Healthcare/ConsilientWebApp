using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;

namespace Consilient.Shared.Contracts;

public interface IFacilityService
{
    Task<IEnumerable<FacilityDto>> GetAllAsync();
    Task<FacilityDto?> GetByIdAsync(int id);
    Task<FacilityDto> CreateAsync(CreateFacilityRequest request);
    Task<FacilityDto?> UpdateAsync(int id, UpdateFacilityRequest request);
    Task<bool> DeleteAsync(int id);
}
