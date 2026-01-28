using Consilient.Common;
using Consilient.Data.Entities.Clinical;

namespace Consilient.Data.Entities.Billing;

public class VisitServiceBilling : BaseEntity<int>
{
    public int VisitId { get; set; }

    public ServiceType Type { get; set; }

    public virtual ServiceTypeEntity ServiceTypeNavigation { get; set; } = null!;

    public int BillingCodeId { get; set; }

    public virtual Visit Visit { get; set; } = null!;

    public virtual BillingCode BillingCode { get; set; } = null!;
}
