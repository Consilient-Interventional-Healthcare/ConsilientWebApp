namespace Consilient.Data.Entities.Clinical
{
    public class HospitalizationStatus : BaseEntity<int>
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BillingCode { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }
}