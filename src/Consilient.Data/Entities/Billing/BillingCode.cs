namespace Consilient.Data.Entities.Billing;

public class BillingCode : BaseEntity<int>
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
