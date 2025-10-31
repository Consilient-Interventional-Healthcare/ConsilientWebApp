namespace Consilient.WebApp.ViewModels
{
    public class ProviderPayViewModel
    {
        public int ProviderPayId { get; set; }

        public int EmployeeId { get; set; }

        public int FacilityId { get; set; }

        public int? ServiceTypeId { get; set; }

        public string Description { get; set; } = null!;

        public decimal PayAmount { get; set; }

        public string PayUnit { get; set; } = null!;

        public virtual EmployeeViewModel Employee { get; set; } = new EmployeeViewModel();

        public virtual FacilityViewModel Facility { get; set; } = new FacilityViewModel();

        //public virtual ICollection<PayrollDatumViewModel> PayrollData { get; set; } = [];

        public virtual ServiceTypeViewModel ServiceType { get; set; } = new ServiceTypeViewModel();
    }
}
