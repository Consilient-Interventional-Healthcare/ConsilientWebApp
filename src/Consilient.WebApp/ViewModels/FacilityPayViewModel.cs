namespace Consilient.WebApp.ViewModels
{
    public class FacilityPayViewModel
    {
        public int FacilityPayId { get; set; }

        public int FacilityId { get; set; }

        public int ServiceTypeId { get; set; }

        public decimal RevenueAmount { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public virtual FacilityViewModel Facility { get; set; } = null!;

        public virtual ServiceTypeViewModel ServiceType { get; set; } = null!;
    }
}
