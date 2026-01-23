using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Consilient.Data.Entities
{
    public class ProviderAssignment : BaseEntity<int>
    {
        public int Age { get; set; }
        public string AttendingMD { get; set; } = string.Empty;
        public string HospitalNumber { get; set; } = string.Empty;
        public DateTime Admit { get; set; }
        public DateOnly? Dob { get; set; }
        public int FacilityId { get; set; }
        public string Mrn { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Insurance { get; set; } = string.Empty;
        public string NursePractitioner { get; set; } = string.Empty;
        public string IsCleared { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateOnly ServiceDate { get; set; }
        public string H_P { get; set; } = string.Empty;
        public string PsychEval { get; set; } = string.Empty;


        public Guid BatchId { get; set; }


        /* Parsed */
        public string? NormalizedPatientLastName { get; set; }
        public string? NormalizedPatientFirstName { get; set; }
        public string? NormalizedPhysicianLastName { get; set; }
        public string? NormalizedNursePractitionerLastName { get; set; }
        public string? Room { get; set; }
        public string? Bed { get; set; } 

        /* Resolved */
        public int? ResolvedPhysicianId { get; set; }
        public int? ResolvedHospitalizationId { get; set; }
        public int? ResolvedPatientId { get; set; }
        public int? ResolvedNursePractitionerId { get; set; }
        public int? ResolvedVisitId { get; set; }
        public int? ResolvedHospitalizationStatusId { get; set; }

        /* Navigation Properties */
        public virtual Patient? ResolvedPatient { get; set; }
        public virtual Provider? ResolvedPhysician { get; set; }
        public virtual Provider? ResolvedNursePractitioner { get; set; }
        public virtual Hospitalization? ResolvedHospitalization { get; set; }
        public virtual Visit? ResolvedVisit { get; set; }
        public virtual HospitalizationStatus? ResolvedHospitalizationStatus { get; set; }

        /* Import Statuses */
        public bool ShouldImport { get; set; }
        public bool Imported { get; set; }
        public string? ValidationErrorsJson { get; set; }
        public string? ExclusionReason { get; set; }
        public bool PatientWasCreated { get; set; }
        public bool PatientFacilityWasCreated { get; set; }
        public bool PhysicianWasCreated { get; set; }
        public bool NursePractitionerWasCreated { get; set; }
        public bool HospitalizationWasCreated { get; set; }


        [NotMapped]
        public List<string> ValidationErrors
        {
            get => string.IsNullOrEmpty(ValidationErrorsJson)
                ? []
                : JsonSerializer.Deserialize<List<string>>(ValidationErrorsJson) ?? [];
            set => ValidationErrorsJson = value.Count == 0
                ? null
                : JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public bool HasValidationErrors => !string.IsNullOrEmpty(ValidationErrorsJson);

        public void AddValidationError(string error)
        {
            var errors = ValidationErrors;
            errors.Add(error);
            ValidationErrors = errors;
        }

        public void AddValidationErrors(IEnumerable<string> errors)
        {
            var currentErrors = ValidationErrors;
            currentErrors.AddRange(errors);
            ValidationErrors = currentErrors;
        }
    }
}
