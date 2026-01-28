using Consilient.Common;

namespace Consilient.Data.Entities
{
    public class Contract
    {
        public int ContractId { get; set; }

        public string? ContractName { get; set; }

        public int EmployeeId { get; set; }

        public int FacilityId { get; set; }

        public ServiceType Type { get; set; }

        public virtual ServiceTypeEntity ServiceTypeNavigation { get; set; } = null!;

        public string PayType { get; set; } = null!;

        public bool WeekendFlag { get; set; }

        public bool SupervisingFlag { get; set; }

        public decimal? Amount { get; set; }

        public virtual Employee Employee { get; set; } = null!;

        public virtual Facility Facility { get; set; } = null!;

        //public virtual ICollection<ProviderContract> ProviderContracts { get; set; } = [];
    }
}
