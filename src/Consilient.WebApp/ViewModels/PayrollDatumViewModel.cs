namespace Consilient.WebApp.ViewModels
{
    public class PayrollDatumViewModel
    {
        public int PayrollDataId { get; set; }

        public int PayrollPeriodId { get; set; }

        public int ProviderPayId { get; set; }

        public int Count { get; set; }

        public DateOnly Date { get; set; }

        public virtual PayrollPeriodViewModel PayrollPeriod { get; set; } = new PayrollPeriodViewModel();

        public virtual ProviderPayViewModel ProviderPay { get; set; } = new ProviderPayViewModel();
    }
}
