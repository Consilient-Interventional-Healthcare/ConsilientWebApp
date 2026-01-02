namespace Consilient.Hospitalizations.Contracts.Models
{
    public class HospitalizationStatusDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string BillingCode { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public HospitalizationStatusType Type { get; set; }
        public string? IconName { get; set; }
    }
}
