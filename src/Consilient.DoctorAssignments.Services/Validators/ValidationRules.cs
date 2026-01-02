namespace Consilient.DoctorAssignments.Services.Validators
{
    /// <summary>
    /// Shared validation rules used across import and resolution phases.
    /// </summary>
    public static class ValidationRules
    {
        /// <summary>
        /// Validates that a numeric string field is valid.
        /// </summary>
        public static string? ValidateNumericField(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (!int.TryParse(value, out _))
            {
                return $"{fieldName} must be numeric";
            }

            return null;
        }

        /// <summary>
        /// Validates that a date is not in the future.
        /// </summary>
        public static string? ValidateDateNotFuture(DateTime? date, string fieldName, DateTime now)
        {
            if (date.HasValue && date.Value > now)
            {
                return $"{fieldName} cannot be in the future";
            }

            return null;
        }

        /// <summary>
        /// Validates MRN field (required + numeric).
        /// </summary>
        public static List<string> ValidateMrn(string? mrn)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(mrn))
            {
                errors.Add("MRN is required");
            }
            else
            {
                var error = ValidateNumericField(mrn, "MRN");
                if (error != null)
                {
                    errors.Add(error);
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates Hospital Number field (required + numeric).
        /// </summary>
        public static List<string> ValidateHospitalNumber(string? hospitalNumber)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(hospitalNumber))
            {
                errors.Add("Hospital Number is required");
            }
            else
            {
                var error = ValidateNumericField(hospitalNumber, "Hospital Number");
                if (error != null)
                {
                    errors.Add(error);
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates date fields (Admit and DOB).
        /// </summary>
        public static List<string> ValidateDateFields(DateTime admit, DateTime? dob, DateTime now)
        {
            var errors = new List<string>();

            var admitError = ValidateDateNotFuture(admit, "Admit date", now);
            if (admitError != null)
            {
                errors.Add(admitError);
            }

            var dobError = ValidateDateNotFuture(dob, "Date of birth", now);
            if (dobError != null)
            {
                errors.Add(dobError);
            }

            return errors;
        }
    }
}
