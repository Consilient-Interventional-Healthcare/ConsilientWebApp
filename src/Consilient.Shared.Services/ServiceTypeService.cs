using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Shared.Services;

public class ServiceTypeService(ConsilientDbContext dataContext) : IServiceTypeService
{
    public async Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = request.Adapt<ServiceTypeEntity>();
        try
        {
            await dataContext.ServiceTypes.AddAsync(entity);
            await dataContext.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to create service type. Database constraint or integrity error occurred.", ex);
        }

        return entity.Adapt<ServiceTypeDto>();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            return false;
        }

        try
        {
            var affected = await dataContext.ServiceTypes
                .Where(st => st.Id == id)
                .ExecuteDeleteAsync();

            return affected > 0;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to delete service type. Related data or database constraints may prevent deletion.", ex);
        }
    }

    public async Task<IEnumerable<ServiceTypeDto>> GetAllAsync()
    {
        var serviceTypes = await dataContext.ServiceTypes
            .AsNoTracking()
            .ToListAsync();

        var billingCodeAssociations = await dataContext.ServiceTypeBillingCodes
            .AsNoTracking()
            .Include(stbc => stbc.BillingCode)
            .ToListAsync();

        var dtos = serviceTypes.Select(st => new ServiceTypeDto
        {
            Id = st.Id,
            Code = st.Code,
            Name = st.Name,
            DisplayOrder = st.DisplayOrder,
            BillingCodes = billingCodeAssociations
                .Where(bc => bc.ServiceTypeId == st.Id)
                .Select(bc => new BillingCodeAssociationDto
                {
                    Code = bc.BillingCode.Code,
                    IsDefault = bc.IsDefault
                })
                .ToList()
        });

        return dtos;
    }

    public async Task<ServiceTypeDto?> GetByIdAsync(int id)
    {
        var dto = await dataContext.ServiceTypes
            .AsNoTracking()
            .Where(st => st.Id == id)
            .ProjectToType<ServiceTypeDto>()
            .FirstOrDefaultAsync();

        return dto;
    }

    public async Task<ServiceTypeDto?> UpdateAsync(int id, UpdateServiceTypeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (id <= 0)
        {
            return null;
        }

        try
        {
            var affected = await dataContext.ServiceTypes
                .Where(st => st.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(st => st.Name, st => request.Description ?? st.Name)
                );

            if (affected == 0)
            {
                return null;
            }

            return await dataContext.ServiceTypes
                .AsNoTracking()
                .Where(st => st.Id == id)
                .ProjectToType<ServiceTypeDto>()
                .FirstOrDefaultAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Failed to update service type. Database constraint or concurrency issue may have occurred.", ex);
        }
    }
}
