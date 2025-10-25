using Consilient.Data;
using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Shared.Services
{
    public class FacilityService(ConsilientDbContext dataContext) : IFacilityService
    {
        private readonly ConsilientDbContext _dataContext = dataContext;

        public async Task<FacilityDto> CreateAsync(CreateFacilityRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<Facility>();
            try
            {
                await _dataContext.Facilities.AddAsync(entity);
                await _dataContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to create facility. Database constraint or integrity error occurred.", ex);
            }

            return entity.Adapt<FacilityDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }

            try
            {
                var affected = await _dataContext.Facilities
                    .Where(f => f.FacilityId == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete facility. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<FacilityDto>> GetAllAsync()
        {
            var dtos = await _dataContext.Facilities
                .AsNoTracking()
                .ProjectToType<FacilityDto>()
                .ToListAsync();

            return dtos;
        }

        public async Task<FacilityDto?> GetByIdAsync(int id)
        {
            var dto = await _dataContext.Facilities
                .AsNoTracking()
                .Where(f => f.FacilityId == id)
                .ProjectToType<FacilityDto>()
                .FirstOrDefaultAsync();

            return dto;
        }

        public async Task<FacilityDto?> UpdateAsync(int id, UpdateFacilityRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (id <= 0)
            {
                return null;
            }

            try
            {
                var affected = await _dataContext.Facilities
                    .Where(f => f.FacilityId == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(f => f.FacilityName, f => request.FacilityName ?? f.FacilityName)
                        .SetProperty(f => f.FacilityAbbreviation, f => request.FacilityAbbreviation ?? f.FacilityAbbreviation)
                    );

                if (affected == 0)
                {
                    return null;
                }

                // Return the updated DTO via a no-tracking projection.
                return await _dataContext.Facilities
                    .AsNoTracking()
                    .Where(f => f.FacilityId == id)
                    .ProjectToType<FacilityDto>()
                    .FirstOrDefaultAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update facility. Database constraint or concurrency issue may have occurred.", ex);
            }
        }
    }
}
