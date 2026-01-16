using Consilient.Data;
using Consilient.Data.Entities;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Visits.Services
{
    internal class VisitService(ConsilientDbContext dataContext) : IVisitService
    {
        public async Task<bool> AddAttendant(int visitId, int attendantId)
        {
            if (visitId <= 0 || attendantId <= 0)
            {
                return false;
            }

            // Avoid duplicates
            var alreadyAttached = await dataContext.Set<VisitAttendant>()
                .AsNoTracking()
                .AnyAsync(va => va.VisitId == visitId && va.ProviderId == attendantId);
            if (alreadyAttached)
            {
                return false;
            }

            // Ensure referenced Visit and Provider exist
            var visitExists = await dataContext.Visits
                .AsNoTracking()
                .AnyAsync(v => v.Id == visitId);
            if (!visitExists)
            {
                return false;
            }

            var providerExists = await dataContext.Providers
                .AsNoTracking()
                .AnyAsync(p => p.Id == attendantId);
            if (!providerExists)
            {
                return false;
            }

            var entity = new VisitAttendant
            {
                VisitId = visitId,
                ProviderId = attendantId
            };

            try
            {
                dataContext.Set<VisitAttendant>().Add(entity);
                await dataContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add visit attendant. Related data or database constraints may prevent insertion.", ex);
            }
        }

        public async Task<VisitDto?> CreateAsync(CreateVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var entity = request.Adapt<Visit>();
            dataContext.Visits.Add(entity);
            await dataContext.SaveChangesAsync();

            return entity.Adapt<VisitDto>();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return false;
            }
            try
            {
                var affected = await dataContext.Visits
                    .Where(e => e.Id == id)
                    .ExecuteDeleteAsync();

                return affected > 0;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to delete employee. Related data or database constraints may prevent deletion.", ex);
            }
        }

        public async Task<IEnumerable<VisitDto>> GetByDateAsync(DateOnly date)
        {
            var Visits = await dataContext.Visits
                .AsNoTracking()
                .Where(e => e.DateServiced == date)
                .ProjectToType<VisitDto>()
                .ToListAsync();
            return Visits;
        }

        public async Task<IEnumerable<VisitDto>> GetByProviderAsync(int providerId)
        {
            var Visits = await dataContext.Visits
                    .AsNoTracking()
                    .Where(e => e.VisitAttendants.Any(m => m.ProviderId == providerId))
                    .ProjectToType<VisitDto>()
                    .ToListAsync();
            return Visits;
        }

        public async Task<VisitDto?> GetByIdAsync(int id)
        {
            var Visit = await dataContext.Visits.FindAsync(id);
            return Visit?.Adapt<VisitDto>();
        }

        public async Task<VisitDto?> UpdateAsync(int id, UpdateVisitRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var affected = await dataContext.Visits
                .Where(e => e.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(e => e.IsScribeServiceOnly, _ => request.IsScribeServiceOnly)
                    .SetProperty(e => e.ServiceTypeId, _ => request.ServiceTypeId)
                );

            if (affected == 0)
            {
                return null;
            }

            return await dataContext.Visits
                .AsNoTracking()
                .Where(e => e.Id == id)
                .ProjectToType<VisitDto>()
                .FirstOrDefaultAsync();
        }
    }
}
