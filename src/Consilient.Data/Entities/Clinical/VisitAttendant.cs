namespace Consilient.Data.Entities.Clinical
{
    public class VisitAttendant : BaseEntity<int>
    {
        public int VisitId { get; set; }
        public int ProviderId { get; set; }
        public virtual Visit Visit { get; set; } = null!;
        public virtual Provider Provider { get; set; } = null!;
    }
}
