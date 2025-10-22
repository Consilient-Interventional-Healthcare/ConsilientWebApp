namespace Consilient.WebApp.Models;

public partial class Insurance
{
    public int InsuranceId { get; set; }

    public string? InsuranceCode { get; set; }

    public string? InsuranceDescription { get; set; }

    public bool? PhysicianIncluded { get; set; }

    public bool? IsContracted { get; set; }

    public string CodeAndDescription { get; set; } = null!;

    public virtual ICollection<PatientVisit> PatientVisits { get; set; } = [];

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagings { get; set; } = [];
}
