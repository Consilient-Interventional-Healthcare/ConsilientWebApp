using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Contracts.Requests;

namespace Consilient.Insurances.Contracts;

public interface IInsuranceService
{
    Task<InsuranceDto> CreateAsync(CreateInsuranceRequest request);

    Task<IEnumerable<InsuranceDto>> GetAllAsync();
    public Task<InsuranceDto?> GetByIdAsync(int id);
    Task<InsuranceDto?> UpdateAsync(int id, UpdateInsuranceRequest request);
}
