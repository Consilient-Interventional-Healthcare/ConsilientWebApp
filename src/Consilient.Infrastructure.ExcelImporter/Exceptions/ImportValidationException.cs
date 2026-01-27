using Consilient.Infrastructure.ExcelImporter.Contracts;

namespace Consilient.Infrastructure.ExcelImporter.Exceptions;


public class ImportValidationException : Exception
{
    public List<ValidationError> ValidationErrors { get; }

    public ImportValidationException(List<ValidationError> errors)
        : base($"Import validation failed with {errors.Count} error(s)")
    {
        ValidationErrors = errors;
    }
}