using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Services.Validators;
using Consilient.Infrastructure.ExcelImporter.Models;
using Consilient.Infrastructure.ExcelImporter.Validators;

namespace Consilient.ProviderAssignments.Services.Importer
{
    public class ProviderAssignmentValidator(Func<DateTime>? utcNow = null) : RowValidator<ExternalProviderAssignment>
    {
        private readonly Func<DateTime> _utcNow = utcNow ?? (() => DateTime.UtcNow);

        public override ValidationResult Validate(ExternalProviderAssignment row, int rowNumber)
        {
            var errors = new List<ValidationError>();
            var now = _utcNow();

            if (string.IsNullOrWhiteSpace(row.Name))
            {
                errors.Add(Error(rowNumber, nameof(row.Name), "Name is required"));
            }

            if (row.Age < 0 || row.Age > 150)
            {
                errors.Add(Error(rowNumber, nameof(row.Age), "Age must be between 0 and 150"));
            }

            // Use shared validation rules
            var hospitalNumberErrors = ValidationRules.ValidateHospitalNumber(row.HospitalNumber);
            foreach (var errorMsg in hospitalNumberErrors)
            {
                errors.Add(Error(rowNumber, nameof(row.HospitalNumber), errorMsg));
            }

            var dateErrors = ValidationRules.ValidateDateFields(row.Admit, row.Dob, now);
            foreach (var errorMsg in dateErrors)
            {
                var fieldName = errorMsg.Contains("Admit") ? nameof(row.Admit) : nameof(row.Dob);
                errors.Add(Error(rowNumber, fieldName, errorMsg));
            }

            var mrnErrors = ValidationRules.ValidateMrn(row.Mrn);
            foreach (var errorMsg in mrnErrors)
            {
                errors.Add(Error(rowNumber, nameof(row.Mrn), errorMsg));
            }

            return errors.Count > 0
                ? ValidationResult.Failed([.. errors])
                : ValidationResult.Success();
        }
    }
}
