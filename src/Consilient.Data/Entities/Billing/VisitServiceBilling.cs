using Consilient.Data.Entities.Clinical;

namespace Consilient.Data.Entities.Billing;

public class VisitServiceBilling : BaseEntity<int>
{
    public int VisitId { get; set; }
    public int ServiceTypeId { get; set; }
    public int BillingCodeId { get; set; }

    public virtual Visit Visit { get; set; } = null!;
    public virtual ServiceType ServiceType { get; set; } = null!;
    public virtual BillingCode BillingCode { get; set; } = null!;
}
