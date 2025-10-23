namespace Consilient.Data;

public partial class FacilityPay
{
    public int FacilityPayId { get; set; }

    public int FacilityId { get; set; }

    public int ServiceTypeId { get; set; }

    public decimal RevenueAmount { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual Facility Facility { get; set; } = null!;

    public virtual ServiceType ServiceType { get; set; } = null!;
}
