namespace Consilient.Data.Entities.Clinical
{
    public class HospitalizationInsurance : BaseEntity<int>
    {
        public int HospitalizationId { get; set; }
        public int InsuranceId { get; set; }
        public DateTime StartDate { get; set; }
        public virtual Hospitalization Hospitalization { get; set; } = null!;
        public virtual Insurance Insurance { get; set; } = null!;
    }
}
