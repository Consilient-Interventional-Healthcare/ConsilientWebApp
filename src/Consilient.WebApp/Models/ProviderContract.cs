namespace Consilient.WebApp.Models;

public partial class ProviderContract
{
    public int ProviderContractId { get; set; }

    public int EmployeeId { get; set; }

    public int ContractId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual Contract Contract { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;
}
