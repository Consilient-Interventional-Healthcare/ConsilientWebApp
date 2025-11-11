namespace Consilient.Data.Entities
{
    public class Hospitalization : BaseEntity<int>
    {
        public int PatientId { get; set; }
        public int CaseId { get; set; }
        public int FacilityId { get; set; }
        public DateOnly AdmissionDate { get; set; }
        public DateOnly? DischargeDate { get; set; }

        public virtual Patient Patient { get; set; } = null!;
        public virtual Facility Facility { get; set; } = null!;
    }
}
