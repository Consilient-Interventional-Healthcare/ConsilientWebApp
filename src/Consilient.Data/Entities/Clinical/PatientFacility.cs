namespace Consilient.Data.Entities.Clinical
{
    public class PatientFacility : BaseEntity<int>
    {
        public int PatientId { get; set; }
        public int FacilityId { get; set; }
        public string Mrn { get; set; } = string.Empty;

        public virtual Patient Patient { get; set; } = null!;
        public virtual Facility Facility { get; set; } = null!;
    }
}
