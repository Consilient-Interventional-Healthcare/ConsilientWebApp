using Consilient.Infrastructure.ExcelImporter.Domain;
using Consilient.Infrastructure.ExcelImporter.Models;

namespace Consilient.Infrastructure.ExcelImporter.Validators
{
    public class DoctorAssignmentValidator : RowValidator<DoctorAssignment>
    {
        public override ValidationResult Validate(DoctorAssignment row, int rowNumber)
        {
            var errors = new List<ValidationError>();

            if (string.IsNullOrWhiteSpace(row.Name))
            {
                errors.Add(Error(rowNumber, nameof(row.Name), "Name is required"));
            }

            if (row.Age < 0 || row.Age > 150)
            {
                errors.Add(Error(rowNumber, nameof(row.Age), "Age must be between 0 and 150"));
            }

            if (string.IsNullOrWhiteSpace(row.HospitalNumber))
            {
                errors.Add(Error(rowNumber, nameof(row.HospitalNumber), "Hospital Number is required"));
            }

            if (row.Admit > DateTime.UtcNow)
            {
                errors.Add(Error(rowNumber, nameof(row.Admit), "Admit date cannot be in the future"));
            }

            if (row.Dob.HasValue && row.Dob.Value > DateTime.UtcNow)
            {
                errors.Add(Error(rowNumber, nameof(row.Dob), "Date of birth cannot be in the future"));
            }

            if (string.IsNullOrWhiteSpace(row.Mrn))
            {
                errors.Add(Error(rowNumber, nameof(row.Mrn), "MRN is required"));
            }

            return errors.Count > 0
                ? ValidationResult.Failed([.. errors])
                : ValidationResult.Success();
        }
    }
}
