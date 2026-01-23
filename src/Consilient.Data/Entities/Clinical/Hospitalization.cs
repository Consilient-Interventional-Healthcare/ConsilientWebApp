namespace Consilient.Data.Entities.Clinical
{
    public class Hospitalization : BaseEntity<int>
    {
        public int PatientId { get; set; }
        public int CaseId { get; set; }
        public int FacilityId { get; set; }
        public bool PsychEvaluation { get; set; }
        public DateTime AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public int HospitalizationStatusId { get; set; }

        public virtual HospitalizationStatus HospitalizationStatus { get; set; } = null!;
        public virtual Patient Patient { get; set; } = null!;
        public virtual Facility Facility { get; set; } = null!;
    }
}
