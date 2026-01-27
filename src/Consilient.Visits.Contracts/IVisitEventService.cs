using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;

namespace Consilient.Visits.Contracts;

public interface IVisitEventService
{
    Task<int> InsertVisitEventAsync(InsertVisitEventRequest request, int userId, CancellationToken cancellationToken = default);
    Task<int> DeleteVisitEventAsync(DeleteVisitEventRequest request, CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitEventTypeDto>> GetVisitEventTypesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<VisitEventDto>> GetVisitEventsByVisitId(int visitId, CancellationToken cancellationToken = default);
}
