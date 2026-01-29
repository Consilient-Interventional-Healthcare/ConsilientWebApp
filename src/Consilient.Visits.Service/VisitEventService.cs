using Consilient.Data;
using Consilient.Data.Entities.Clinical;
using Consilient.Visits.Contracts;
using Consilient.Visits.Contracts.Models;
using Consilient.Visits.Contracts.Models.Requests;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Consilient.Visits.Services;

internal class VisitEventService(ConsilientDbContext dataContext) : IVisitEventService
{
    public async Task<int> DeleteVisitEventAsync(DeleteVisitEventRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.VisitEventId <= 0)
        {
            return 0;
        }

        var affected = await dataContext.VisitEvents
            .Where(e => e.Id == request.VisitEventId)
            .ExecuteDeleteAsync(cancellationToken);

        return affected;
    }

    public Task<IEnumerable<VisitEventDto>> GetVisitEventsByVisitId(int visitId, CancellationToken cancellationToken = default)
    {
        return dataContext.VisitEvents
            .AsNoTracking()
            .Where(e => e.VisitId == visitId)
            .OrderByDescending(e => e.EventOccurredAt)
            .ProjectToType<VisitEventDto>()
            .ToListAsync(cancellationToken)
            .ContinueWith(t => (IEnumerable<VisitEventDto>)t.Result, cancellationToken);
    }

    public async Task<IEnumerable<VisitEventTypeDto>> GetVisitEventTypesAsync(CancellationToken cancellationToken = default)
    {
        var eventTypes = await dataContext.VisitEventTypes
            .AsNoTracking()
            .ProjectToType<VisitEventTypeDto>()
            .ToListAsync(cancellationToken);

        return eventTypes;
    }

    public async Task<int> InsertVisitEventAsync(InsertVisitEventRequest request, int userId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new VisitEvent
        {
            VisitId = request.VisitId,
            EventType = (VisitEventType)request.EventTypeId,
            EventOccurredAt = DateTime.UtcNow,
            Description = request.Description,
            EnteredByUserId = userId
        };

        dataContext.VisitEvents.Add(entity);
        await dataContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
