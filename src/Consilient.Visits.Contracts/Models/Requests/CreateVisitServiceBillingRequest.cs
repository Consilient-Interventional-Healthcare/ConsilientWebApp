namespace Consilient.Visits.Contracts.Models.Requests;

public class CreateVisitServiceBillingRequest
{
    public int VisitId { get; set; }
    public int ServiceTypeId { get; set; }
    public int BillingCodeId { get; set; }
}
