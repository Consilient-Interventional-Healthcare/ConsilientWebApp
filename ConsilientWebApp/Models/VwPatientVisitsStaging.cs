using System;
using System.Collections.Generic;

namespace ConsilientWebApp.Models;

public partial class VwPatientVisitsStaging
{
    public int PatientVisitStagingId { get; set; }

    public DateOnly DateServiced { get; set; }

    public string? PatientName { get; set; }

    public string? FacilityName { get; set; }

    public string? Insurance { get; set; }

    public string? ServiceType { get; set; }

    public string? Physician { get; set; }

    public string? NursePractitioner { get; set; }

    public string? Scribe { get; set; }
}
