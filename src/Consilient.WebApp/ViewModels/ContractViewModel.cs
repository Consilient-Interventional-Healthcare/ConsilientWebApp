namespace Consilient.WebApp.ViewModels
{
    public class ContractViewModel
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

        public virtual EmployeeViewModel Employee { get; set; } = null!;

        public virtual FacilityViewModel Facility { get; set; } = null!;

        //public virtual ICollection<ProviderContractViewModel> ProviderContracts { get; set; } = [];

        public virtual ServiceTypeViewModel ServiceType { get; set; } = null!;
    }
}
