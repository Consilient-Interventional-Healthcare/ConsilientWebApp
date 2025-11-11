namespace Consilient.Data.Entities
{

    public class ServiceType : BaseEntity<int>
    {
        //public int ServiceTypeId { get; set; }

        public string? Description { get; set; }

        public int? Cptcode { get; set; }

        //public virtual ICollection<Contract> Contracts { get; set; } = [];

        //public virtual ICollection<FacilityPay> FacilityPays { get; set; } = [];

        //public virtual ICollection<PatientVisit> PatientVisits { get; set; } = [];

        //public virtual ICollection<PatientVisitStaging> PatientVisitsStagings { get; set; } = [];

        //public virtual ICollection<ProviderPay> ProviderPays { get; set; } = [];
    }
}