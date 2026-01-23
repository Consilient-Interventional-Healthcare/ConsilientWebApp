namespace Consilient.Data.Entities.Compensation
{

    public class ProviderContract : BaseEntity<int>
    {

        public int EmployeeId { get; set; }
        public int FacilityId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}