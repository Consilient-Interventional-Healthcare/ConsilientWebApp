using Consilient.Data;
using Consilient.Insurances.Contracts;
using Consilient.Insurances.Contracts.Dtos;
using Mapster;

namespace Consilient.Insurances.Services
{
    public class InsuranceService(ConsilientDbContext dataContext) : IInsuranceService
    {
        private readonly ConsilientDbContext DataContext = dataContext;

        public async Task<InsuranceDto?> GetById(int id)
        {
            var insurance = await DataContext.Insurances.FindAsync(id);
            return insurance?.Adapt<InsuranceDto>();
        }
    }
}
