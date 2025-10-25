using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;

namespace Consilient.Api.Client.Contracts
{
    public interface IServiceTypesApi : IApi
    {
        Task<IEnumerable<ServiceTypeDto>> GetAllAsync();

        Task<ServiceTypeDto?> GetByIdAsync(int id);

        Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request);

        Task<ServiceTypeDto?> UpdateAsync(int id, UpdateServiceTypeRequest request);

        Task<bool> DeleteAsync(int id);
    }
}