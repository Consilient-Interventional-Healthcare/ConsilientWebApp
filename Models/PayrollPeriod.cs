using System;
using System.Collections.Generic;

namespace ConsilientWebApp.Models;

public partial class PayrollPeriod
{
    public int PayrollPeriodId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public DateOnly PayrollDate { get; set; }

    public DateOnly? PayrollProcessingStartDate { get; set; }

    public virtual ICollection<PayrollDatum> PayrollData { get; set; } = new List<PayrollDatum>();
}
