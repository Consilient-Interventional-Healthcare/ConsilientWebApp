namespace Consilient.Data.Entities.Clinical
{
    public class Visit : BaseEntity<int>
    {
        public DateOnly DateServiced { get; set; }
        public virtual Hospitalization Hospitalization { get; set; } = null!;
        public int HospitalizationId { get; set; }
        public bool IsScribeServiceOnly { get; set; }
        public string Room { get; set; } = string.Empty;
        public string Bed { get; set; } = string.Empty;
        public virtual ICollection<VisitAttendant> VisitAttendants { get; set; } = null!;
        public virtual ICollection<VisitEvent> VisitEvents { get; set; } = null!;
    }
}