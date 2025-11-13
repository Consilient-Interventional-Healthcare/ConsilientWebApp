namespace Consilient.Data.Entities
{
    public class VisitAttendant : BaseEntity<int>
    {
        public int VisitId { get; set; }
        public int EmployeeId { get; set; }
        public virtual Visit Visit { get; set; } = null!;
        public virtual Employee Employee { get; set; } = null!;
    }
}
