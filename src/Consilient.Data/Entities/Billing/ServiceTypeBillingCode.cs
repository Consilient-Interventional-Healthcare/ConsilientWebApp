using Consilient.Common;
using Consilient.Data.Entities.Clinical;

namespace Consilient.Data.Entities.Billing;

/// <summary>
/// Defines valid (ServiceType, BillingCode) pairings.
/// One record per ServiceTypeId should have IsDefault = true.
/// </summary>
public class ServiceTypeBillingCode : BaseEntity<int>
{
    public int ServiceTypeId { get; set; }
    public virtual ServiceTypeEntity ServiceType { get; set; } = null!;

    public int BillingCodeId { get; set; }
    public virtual BillingCode BillingCode { get; set; } = null!;

    /// <summary>
    /// True if this is the default billing code for this service type.
    /// </summary>
    public bool IsDefault { get; set; }
}
