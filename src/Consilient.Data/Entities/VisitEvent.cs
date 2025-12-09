namespace Consilient.Data.Entities
{
    public class VisitEvent : BaseEntity<int>
    {
        public int VisitId { get; set; }

        public int EventTypeId { get; set; }

        public DateTime EventOccurredAt { get; set; }

        public DateTime EventRecordedAt { get; set; }

        public string Description { get; set; } = string.Empty;

        public int EnteredByEmployeeId { get; set; }

        public virtual Visit Visit { get; set; } = null!;

        public virtual VisitEventType EventType { get; set; } = null!;

        public virtual Employee EnteredByEmployee { get; set; } = null!;
    }
}
