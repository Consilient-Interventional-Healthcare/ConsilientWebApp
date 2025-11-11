namespace Consilient.Data.Entities
{

    public class Insurance : BaseEntity<int>
    {
        //public int InsuranceId { get; set; }

        public string? InsuranceCode { get; set; }

        public string? InsuranceDescription { get; set; }

        public bool? PhysicianIncluded { get; set; }

        public bool? IsContracted { get; set; }

        //public virtual ICollection<PatientVisit> PatientVisits { get; set; } = [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagings { get; set; } = [];
    }
}