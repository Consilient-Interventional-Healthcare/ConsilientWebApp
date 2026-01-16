namespace Consilient.ProviderAssignments.Services.Models
{
    public class StagedProviderAssignment
    {
        public int Id { get; set; }
        public int Age { get; set; }
        public string AttendingMD { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime Admit { get; set; }
        public DateOnly? Dob { get; set; }
        public int? FacilityId { get; set; }
        public string Mrn { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Insurance { get; set; } = string.Empty;
        public string NursePractitioner { get; set; } = string.Empty;
        public string IsCleared { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateOnly? ServiceDate { get; set; }
        public string H_P { get; set; } = string.Empty;
        public string PsychEval { get; set; } = string.Empty;
        public DateTime CreatedAtUTC { get; set; }
        public int? ResolvedProviderId { get; set; }
        public int? ResolvedHospitalizationId { get; set; }
        public int? ResolvedPatientId { get; set; }
        public int? ResolvedNursePracticionerId { get; set; }
        public Guid BatchId { get; set; }
        public bool Imported { get; set; }
        public List<string> ValidationErrors { get; set; } = [];
        public string? ExclusionReason { get; set; }
        public bool ShouldImport { get; set; }
        public bool NeedsNewPatient { get; set; }
        public bool NeedsNewHospitalization { get; set; }
        public int? ResolvedVisitId { get; set; }
    }
}
