using Consilient.Insurances.Contracts.Dtos;

namespace Consilient.Insurances.Contracts
{
    public interface IInsuranceService
    {
        public Task<InsuranceDto?> GetById(int id);
    }
}
