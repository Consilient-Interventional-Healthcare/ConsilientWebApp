using System;
using System.Collections.Generic;

namespace ConsilientWebApp.Models;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string? FacilityName { get; set; }

    public string? FacilityAbbreviation { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<FacilityPay> FacilityPays { get; set; } = new List<FacilityPay>();

    public virtual ICollection<PatientVisit> PatientVisits { get; set; } = new List<PatientVisit>();

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagings { get; set; } = new List<PatientVisitsStaging>();

    public virtual ICollection<ProviderPay> ProviderPays { get; set; } = new List<ProviderPay>();
}
