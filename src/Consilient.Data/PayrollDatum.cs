namespace Consilient.Data;

public partial class PayrollDatum
{
    public int PayrollDataId { get; set; }

    public int PayrollPeriodId { get; set; }

    public int ProviderPayId { get; set; }

    public int Count { get; set; }

    public DateOnly Date { get; set; }

    public virtual PayrollPeriod PayrollPeriod { get; set; } = null!;

    public virtual ProviderPay ProviderPay { get; set; } = null!;
}
