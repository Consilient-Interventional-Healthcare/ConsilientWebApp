namespace Consilient.Data.Entities.Clinical
{
    public class VisitEvent : BaseEntity<int>
    {
        public int VisitId { get; set; }

        public int EventTypeId { get; set; }

        public DateTime EventOccurredAt { get; set; }

        public string Description { get; set; } = string.Empty;

        public int EnteredByUserId { get; set; }
    }
}
