using Consilient.Data;
using Consilient.Shared.Contracts;
using Consilient.Shared.Contracts.Dtos;
using Consilient.Shared.Contracts.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Shared.Services
{
    public class ServiceTypeService(ConsilientDbContext dataContext) : IServiceTypeService
    {
        private readonly ConsilientDbContext _dataContext = dataContext;

        public async Task<ServiceTypeDto> CreateAsync(CreateServiceTypeRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<ServiceType>();
            try
            {
                await _dataContext.ServiceTypes.AddAsync(entity);
                await _dataContext.SaveChangesAsync();
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
                var affected = await _dataContext.ServiceTypes
                    .Where(st => st.ServiceTypeId == id)
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
            var dtos = await _dataContext.ServiceTypes
                .AsNoTracking()
                .ProjectToType<ServiceTypeDto>()
                .ToListAsync();

            return dtos;
        }

        public async Task<ServiceTypeDto?> GetByIdAsync(int id)
        {
            var dto = await _dataContext.ServiceTypes
                .AsNoTracking()
                .Where(st => st.ServiceTypeId == id)
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
                var affected = await _dataContext.ServiceTypes
                    .Where(st => st.ServiceTypeId == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(st => st.Description, st => request.Description ?? st.Description)
                        .SetProperty(st => st.Cptcode, st => request.CptCode ?? st.Cptcode)
                    );

                if (affected == 0)
                {
                    return null;
                }

                return await _dataContext.ServiceTypes
                    .AsNoTracking()
                    .Where(st => st.ServiceTypeId == id)
                    .ProjectToType<ServiceTypeDto>()
                    .FirstOrDefaultAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update service type. Database constraint or concurrency issue may have occurred.", ex);
            }
        }
    }
}
