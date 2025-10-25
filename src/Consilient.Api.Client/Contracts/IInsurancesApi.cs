using Consilient.Insurances.Contracts.Dtos;

namespace Consilient.Api.Client.Contracts
{
    public interface IInsurancesApi
    {
        public Task<InsuranceDto?> GetById(int id);
    }
}
