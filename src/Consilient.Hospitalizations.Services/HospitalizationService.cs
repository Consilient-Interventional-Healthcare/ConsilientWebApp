using Consilient.Data;
using Consilient.Hospitalizations.Contracts;
using Consilient.Hospitalizations.Contracts.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Hospitalizations.Services;

public class HospitalizationService(ConsilientDbContext dataContext) : IHospitalizationService
{
    public Task<HospitalizationDto?> GetHospitalizationById(int id)
    {
        return dataContext.Hospitalizations
            .AsNoTracking()
            .Where(h => h.Id == id)
            .ProjectToType<HospitalizationDto>()
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<HospitalizationStatusDto>> GetHospitalizationStatuses()
    {
        var statuses = await dataContext.HospitalizationStatuses
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .ProjectToType<HospitalizationStatusDto>()
            .ToListAsync();

        return statuses;
    }
}
