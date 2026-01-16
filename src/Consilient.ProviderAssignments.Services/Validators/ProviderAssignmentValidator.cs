using Consilient.Data.Entities;

namespace Consilient.ProviderAssignments.Services.Validators
{
    public class ProviderAssignmentValidator(Func<DateTime>? utcNow = null)
    {
        private readonly Func<DateTime> _utcNow = utcNow ?? (() => DateTime.UtcNow);

        public List<string> Validate(ProviderAssignment record)
        {
            var errors = new List<string>();
            var now = _utcNow();

            // Required field validations
            errors.AddRange(ValidationRules.ValidateMrn(record.Mrn));
            errors.AddRange(ValidationRules.ValidateHospitalNumber(record.HospitalNumber));

            if (string.IsNullOrWhiteSpace(record.AttendingMD))
            {
                errors.Add("Attending MD is required");
            }

            if (record.FacilityId <= 0)
            {
                errors.Add("Facility ID has to be a positive number");
            }

            // Data integrity validations
            errors.AddRange(ValidationRules.ValidateDateFields(record.Admit, record.Dob, now));

            return errors;
        }
    }
}
