using Consilient.Data;
using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Services.Contracts;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Insurances.Services
{
    public class InsuranceService(ConsilientDbContext dataContext) : IInsuranceService
    {
        private readonly ConsilientDbContext DataContext = dataContext;

        public async Task<InsuranceDto?> GetById(int id)
        {
            var insurance = await DataContext.Insurances.FirstOrDefaultAsync(m => m.InsuranceId == id);
            return insurance?.Adapt<InsuranceDto>();
        }
    }
}
