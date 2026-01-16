namespace Consilient.ProviderAssignments.Contracts
{
    public record ExternalProviderAssignment
    {
        public string Name { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public string HospitalNumber { get; init; } = string.Empty;
        public DateTime Admit { get; init; }
        public string Mrn { get; init; } = string.Empty;
        public int Age { get; init; }
        public DateOnly? Dob { get; init; }
        public int FacilityId { get; init; }
        public string Insurance { get; init; } = string.Empty;
        public string NursePractitioner { get; init; } = string.Empty;
        public string IsCleared { get; init; } = string.Empty;
        public DateOnly ServiceDate { get; init; }
        public string H_P { get; init; } = string.Empty;
        public string PsychEval { get; init; } = string.Empty;
        public string AttendingMD { get; init; } = string.Empty;
    }
}
