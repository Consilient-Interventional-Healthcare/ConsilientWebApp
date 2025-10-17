using ConsilientWebApp.Models;

namespace ConsilientWebApp.ViewModels
{
    public class ProviderContractViewModel
    {
        public int ProviderContractId { get; set; }

        public int EmployeeId { get; set; }

        public int ContractId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public virtual ContractViewModel Contract { get; set; } = null!;

        public virtual EmployeeViewModel Employee { get; set; } = null!;
    }
}
