using Consilient.Common;

namespace Consilient.Data.Entities.Clinical;

public class VisitEvent : BaseEntity<int>
{
    public int VisitId { get; set; }

    public VisitEventType EventType { get; set; }

    public virtual VisitEventTypeEntity? EventTypeNavigation { get; set; }

    public DateTime EventOccurredAt { get; set; }

    public string Description { get; set; } = string.Empty;

    public int EnteredByUserId { get; set; }
}
