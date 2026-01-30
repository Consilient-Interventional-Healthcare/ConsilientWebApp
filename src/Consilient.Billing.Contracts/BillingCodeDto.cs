namespace Consilient.Billing.Contracts;

public class BillingCodeDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
