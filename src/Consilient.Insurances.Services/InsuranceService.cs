using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.Insurances.Contracts;
using Consilient.Insurances.Contracts.Dtos;
using Consilient.Insurances.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Insurances.Services
{
    public class InsuranceService(ConsilientDbContext dataContext) : IInsuranceService
    {
        public async Task<InsuranceDto> CreateAsync(CreateInsuranceRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<Insurance>();

            try
            {
                await dataContext.Insurances.AddAsync(entity);
                await dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to create insurance. Database constraint or integrity error occurred.", ex);
            }

            return entity.Adapt<InsuranceDto>();
        }

        public async Task<IEnumerable<InsuranceDto>> GetAllAsync()
        {
            var dtos = await dataContext.Insurances
                .AsNoTracking()
                .ProjectToType<InsuranceDto>()
                .ToListAsync();

            return dtos;
        }

        public async Task<InsuranceDto?> GetByIdAsync(int id)
        {
            var insurance = await dataContext.Insurances.FindAsync(id);
            return insurance?.Adapt<InsuranceDto>();
        }

        public async Task<InsuranceDto?> UpdateAsync(int id, UpdateInsuranceRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var existing = await dataContext.Insurances.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            // Map values from request onto the tracked entity
            request.Adapt(existing);

            try
            {
                await dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update insurance. Database constraint or concurrency issue may have occurred.", ex);
            }

            return existing.Adapt<InsuranceDto>();
        }
    }
}
