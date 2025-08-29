using System;
using System.Collections.Generic;

namespace ConsilientWebApp.Models;

public partial class Employee
{
    public int EmployeeId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? TitleExtension { get; set; }

    public bool IsProvider { get; set; }

    public string? Role { get; set; }

    public string FullName { get; set; } = null!;

    public bool IsAdministrator { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public virtual ICollection<PatientVisit> PatientVisitCosigningPhysicianEmployees { get; set; } = new List<PatientVisit>();

    public virtual ICollection<PatientVisit> PatientVisitNursePractitionerEmployees { get; set; } = new List<PatientVisit>();

    public virtual ICollection<PatientVisit> PatientVisitPhysicianEmployees { get; set; } = new List<PatientVisit>();

    public virtual ICollection<PatientVisit> PatientVisitScribeEmployees { get; set; } = new List<PatientVisit>();

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagingCosigningPhysicianEmployees { get; set; } = new List<PatientVisitsStaging>();

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagingNursePractitionerEmployees { get; set; } = new List<PatientVisitsStaging>();

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagingPhysicianEmployees { get; set; } = new List<PatientVisitsStaging>();

    public virtual ICollection<PatientVisitsStaging> PatientVisitsStagingScribeEmployees { get; set; } = new List<PatientVisitsStaging>();

    public virtual ICollection<ProviderContract> ProviderContracts { get; set; } = new List<ProviderContract>();

    public virtual ICollection<ProviderPay> ProviderPays { get; set; } = new List<ProviderPay>();
}
