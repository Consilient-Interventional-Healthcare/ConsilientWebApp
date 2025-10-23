namespace Consilient.Data;

public partial class ProviderPay
{
    public int ProviderPayId { get; set; }

    public int EmployeeId { get; set; }

    public int FacilityId { get; set; }

    public int? ServiceTypeId { get; set; }

    public string Description { get; set; } = null!;

    public decimal PayAmount { get; set; }

    public string PayUnit { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual Facility Facility { get; set; } = null!;

    public virtual ICollection<PayrollDatum> PayrollData { get; set; } = [];

    public virtual ServiceType? ServiceType { get; set; }
}
