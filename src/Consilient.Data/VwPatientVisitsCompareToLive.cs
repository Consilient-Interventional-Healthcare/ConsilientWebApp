namespace Consilient.Data;

public partial class VwPatientVisitsCompareToLive
{
    public DateOnly ServiceDts { get; set; }

    public string? PatientNm { get; set; }

    public int? Mrn { get; set; }

    public DateOnly? PatientBirthDts { get; set; }

    public int? Cptcd { get; set; }

    public string? InsuranceNm { get; set; }

    public string? AttendingPhysicianJoinId { get; set; }

    public string? NursePractitionerJoinId { get; set; }

    public string? ScribeNm { get; set; }

    public int? ImportFileNm { get; set; }

    public int? ModifiedDts { get; set; }

    public int? CaseId { get; set; }

    public string? CosignPhysicianJoinId { get; set; }
}
