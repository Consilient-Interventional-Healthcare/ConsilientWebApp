using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Consilient.ProviderAssignments.Contracts;

namespace Consilient.ProviderAssignments.Services.Import.Validation.Validators
{
    public class HospitalNumberValidator : RowValidator<ExcelProviderAssignmentRow>, IExcelRowValidator
    {
        public override ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(row.HospitalNumber))
            {
                errors.Add(Error(rowNumber, nameof(row.HospitalNumber), "Hospital Number is required"));
                return ValidationResult.Failed([.. errors]);
            }

            if (!int.TryParse(row.HospitalNumber, out _))
                errors.Add(Error(rowNumber, nameof(row.HospitalNumber), "Hospital Number must be numeric"));

            return errors.Count > 0
                ? ValidationResult.Failed([.. errors])
                : ValidationResult.Success();
        }
    }
}
