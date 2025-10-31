namespace Consilient.WebApp.ViewModels
{
    public class PayrollPeriodViewModel
    {
        public int PayrollPeriodId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public DateOnly PayrollDate { get; set; }

        public DateOnly? PayrollProcessingStartDate { get; set; }

        //public virtual ICollection<PayrollDatumViewModel> PayrollData { get; set; } = [];
    }
}
