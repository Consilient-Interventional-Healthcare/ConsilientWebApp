using System.ComponentModel.DataAnnotations.Schema;

namespace Consilient.Data.Entities.Clinical;

public class VisitEvent : BaseEntity<int>
{
    public int VisitId { get; set; }

    public int EventTypeId { get; set; }

    [NotMapped]
    public VisitEventType EventType
    {
        get => (VisitEventType)EventTypeId;
        set => EventTypeId = (int)value;
    }

    public virtual VisitEventTypeEntity? EventTypeNavigation { get; set; }

    public DateTime EventOccurredAt { get; set; }

    public string Description { get; set; } = string.Empty;

    public int EnteredByUserId { get; set; }
}
