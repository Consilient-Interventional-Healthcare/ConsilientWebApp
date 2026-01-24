using Consilient.Infrastructure.ExcelImporter.Contracts;
using Consilient.Infrastructure.ExcelImporter.Validators;
using Consilient.ProviderAssignments.Contracts;
using Consilient.ProviderAssignments.Contracts.Import;

namespace Consilient.ProviderAssignments.Services.Import.Validation.Validators
{
    public class MrnValidator : RowValidator<ExcelProviderAssignmentRow>, IExcelRowValidator
    {
        public override ValidationResult Validate(ExcelProviderAssignmentRow row, int rowNumber)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(row.Mrn))
            {
                errors.Add(Error(rowNumber, nameof(row.Mrn), "MRN is required"));
                return ValidationResult.Failed([.. errors]);
            }

            if (!int.TryParse(row.Mrn, out _))
                errors.Add(Error(rowNumber, nameof(row.Mrn), "MRN must be numeric"));

            return errors.Count > 0
                ? ValidationResult.Failed([.. errors])
                : ValidationResult.Success();
        }
    }
}
