namespace Consilient.WebApp.Models;

public partial class PatientVisitsStaging
{
    public int PatientVisitStagingId { get; set; }

    public DateOnly DateServiced { get; set; }

    public int PatientId { get; set; }

    public int FacilityId { get; set; }

    public int? AdmissionNumber { get; set; }

    public int? InsuranceId { get; set; }

    public int? ServiceTypeId { get; set; }

    public int PhysicianEmployeeId { get; set; }

    public int? NursePractitionerEmployeeId { get; set; }

    public int? ScribeEmployeeId { get; set; }

    public bool NursePractitionerApproved { get; set; }

    public bool PhysicianApproved { get; set; }

    public string? PhysicianApprovedBy { get; set; }

    public DateTime? PhysicianApprovedDateTime { get; set; }

    public bool AddedToMainTable { get; set; }

    public int? CosigningPhysicianEmployeeId { get; set; }

    public bool IsScribeServiceOnly { get; set; }

    public virtual Employee? CosigningPhysicianEmployee { get; set; }

    public virtual Facility Facility { get; set; } = null!;

    public virtual Insurance? Insurance { get; set; }

    public virtual Employee? NursePractitionerEmployee { get; set; }

    public virtual Patient Patient { get; set; } = null!;

    public virtual Employee PhysicianEmployee { get; set; } = null!;

    public virtual Employee? ScribeEmployee { get; set; }

    public virtual ServiceType? ServiceType { get; set; }
}
