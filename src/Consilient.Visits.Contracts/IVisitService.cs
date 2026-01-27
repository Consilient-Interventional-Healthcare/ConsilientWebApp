using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;

namespace Consilient.Visits.Contracts;

public interface IVisitService<TVisit, in TCreateVisit, in TUpdateVisit>
    where TVisit : class, new()
    where TCreateVisit : class, new()
    where TUpdateVisit : class, new()
{
    Task<bool> AddAttendant(int visitId, int attendantId);

    Task<TVisit?> CreateAsync(TCreateVisit request);

    Task<bool> DeleteAsync(int id);

    Task<IEnumerable<TVisit>> GetByDateAndFacilityIdAsync(DateOnly date, int facilityId);

    Task<IEnumerable<TVisit>> GetByProviderAsync(int providerId);

    Task<TVisit?> GetByIdAsync(int id);

    Task<TVisit?> UpdateAsync(int id, TUpdateVisit request);
}

public interface IVisitService : IVisitService<VisitDto, CreateVisitRequest, UpdateVisitRequest>;
