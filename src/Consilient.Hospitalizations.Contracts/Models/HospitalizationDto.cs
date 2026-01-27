namespace Consilient.Hospitalizations.Contracts.Models;

public class HospitalizationDto
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public int CaseId { get; set; }
    public int FacilityId { get; set; }
    public bool PsychEvaluation { get; set; }
    public DateTime AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public int HospitalizationStatusId { get; set; }
}
