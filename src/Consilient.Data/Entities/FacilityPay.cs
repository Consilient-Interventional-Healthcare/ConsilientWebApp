using System.ComponentModel.DataAnnotations.Schema;
using Consilient.Common;

namespace Consilient.Data.Entities
{
    public class FacilityPay
    {
        public int FacilityPayId { get; set; }

        public int FacilityId { get; set; }

        public int ServiceTypeId { get; set; }

        [NotMapped]
        public ServiceType Type
        {
            get => (ServiceType)ServiceTypeId;
            set => ServiceTypeId = (int)value;
        }

        public virtual ServiceTypeEntity ServiceTypeNavigation { get; set; } = null!;

        public decimal RevenueAmount { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public virtual Facility Facility { get; set; } = null!;
    }
}
