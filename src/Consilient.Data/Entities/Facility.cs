namespace Consilient.Data.Entities
{
    public class Facility : BaseEntity<int>
    {
        //public int FacilityId { get; set; }

        public string? FacilityName { get; set; }

        public string? FacilityAbbreviation { get; set; }

        //public virtual ICollection<Contract> Contracts { get; set; } = [];

        //public virtual ICollection<FacilityPay> FacilityPays { get; set; } = [];

        //public virtual ICollection<PatientVisit> PatientVisits { get; set; } = [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagings { get; set; } = [];

        //public virtual ICollection<ProviderPay> ProviderPays { get; set; } = [];
    }
}