using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;

namespace Consilient.Shared.Contracts
{
    public interface IServiceTypeService
    {
        Task<IEnumerable<ServiceTypeDto>> GetAllAsync();
        Task<ServiceTypeDto?> GetById(int id);
        Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request);
        Task<ServiceTypeDto?> UpdateAsync(int id, UpdateServiceTypeRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
