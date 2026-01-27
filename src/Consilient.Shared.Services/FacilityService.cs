using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Shared.Services;

public class FacilityService(ConsilientDbContext dataContext) : IFacilityService
{
    public async Task<FacilityDto> CreateAsync(CreateFacilityRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = request.Adapt<Facility>();
        try
        {
            await dataContext.Facilities.AddAsync(entity);
            await dataContext.SaveChangesAsync();
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
            var affected = await dataContext.Facilities
                .Where(f => f.Id == id)
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
        var dtos = await dataContext.Facilities
            .AsNoTracking()
            .ProjectToType<FacilityDto>()
            .ToListAsync();

        return dtos;
    }

    public async Task<FacilityDto?> GetByIdAsync(int id)
    {
        var dto = await dataContext.Facilities
            .AsNoTracking()
            .Where(f => f.Id == id)
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
            var affected = await dataContext.Facilities
                .Where(f => f.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Name, f => request.FacilityName ?? f.Name)
                    .SetProperty(f => f.Abbreviation, f => request.FacilityAbbreviation ?? f.Abbreviation)
                );

            if (affected == 0)
            {
                return null;
            }

            // Return the updated DTO via a no-tracking projection.
            return await dataContext.Facilities
                .AsNoTracking()
                .Where(f => f.Id == id)
                .ProjectToType<FacilityDto>()
                .FirstOrDefaultAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to update facility. Database constraint or concurrency issue may have occurred.", ex);
        }
    }
}
