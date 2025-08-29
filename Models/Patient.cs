using System;
using System.Collections.Generic;

namespace ConsilientWebApp.Models;

public partial class Patient
{
    public int PatientId { get; set; }

    public int PatientMrn { get; set; }

    public string? PatientFirstName { get; set; }

    public string? PatientLastName { get; set; }

    public DateOnly? PatientBirthDate { get; set; }

    public string PatientFullName { get; set; } = null!;

    public virtual ICollection<PatientVisit> PatientVisits { get; set; } = new List<PatientVisit>();

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagings { get; set; } = new List<PatientVisitsStaging>();
}
