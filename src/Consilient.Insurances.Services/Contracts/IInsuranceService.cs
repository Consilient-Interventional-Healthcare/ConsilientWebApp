using Consilient.Insurances.Contracts.Dtos;

namespace Consilient.Insurances.Services.Contracts
{
    public interface IInsuranceService
    {
        public Task<InsuranceDto?> GetById(int id);
    }
}
