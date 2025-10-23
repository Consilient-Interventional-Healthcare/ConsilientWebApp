namespace Consilient.Data;

public partial class Contract
{
    public int ContractId { get; set; }

    public string? ContractName { get; set; }

    public int EmployeeId { get; set; }

    public int FacilityId { get; set; }

    public int ServiceTypeId { get; set; }

    public string PayType { get; set; } = null!;

    public bool WeekendFlag { get; set; }

    public bool SupervisingFlag { get; set; }

    public decimal? Amount { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Facility Facility { get; set; } = null!;

    public virtual ICollection<ProviderContract> ProviderContracts { get; set; } = [];

    public virtual ServiceType ServiceType { get; set; } = null!;
}
