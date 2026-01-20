using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.ProviderAssignments.Contracts
{
    /// <summary>
    /// Deprecated. Use <see cref="ExcelProviderAssignmentRow"/> for raw Excel data
    /// and <see cref="ProcessedProviderAssignment"/> for enriched data with import context.
    /// </summary>
    [Obsolete("Use ExcelProviderAssignmentRow for raw data and ProcessedProviderAssignment for enriched data. This type will be removed in a future release.")]
    public record ExternalProviderAssignment : IValidatable
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
        public string? Room { get; set; }
        public string? Bed { get; set; }
        public string? NormalizedPatientLastName { get; set; }
        public string? NormalizedPatientFirstName { get; set; }
        public string? NormalizedPhysicianLastName { get; set; }
        public string? NormalizedNursePractitionerLastName { get; set; }

        /// <summary>
        /// Validation errors collected during import.
        /// </summary>
        public List<string> ValidationErrors { get; set; } = [];
    }
}
