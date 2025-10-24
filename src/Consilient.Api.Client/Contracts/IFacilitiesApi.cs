using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Consilient.Api.Client.Contracts
{
    public interface IFacilitiesApi : IApi
    {
        Task<IEnumerable<FacilityDto>> GetAllAsync();

        Task<FacilityDto?> GetByIdAsync(int id);

        Task<FacilityDto> CreateAsync(CreateFacilityRequest request);

        Task<FacilityDto?> UpdateAsync(int id, UpdateFacilityRequest request);

        Task<bool> DeleteAsync(int id);
    }
}