namespace Consilient.Data.GraphQL.Models;

public class VisitServiceBillingInfo
{
    public int Id { get; set; }
    public int ServiceTypeId { get; set; }
    public string ServiceTypeCode { get; set; } = string.Empty;
    public string ServiceTypeName { get; set; } = string.Empty;
    public int BillingCodeId { get; set; }
    public string BillingCodeCode { get; set; } = string.Empty;
    public string BillingCodeDescription { get; set; } = string.Empty;
}
