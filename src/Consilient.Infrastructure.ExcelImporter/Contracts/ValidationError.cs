namespace Consilient.Infrastructure.ExcelImporter.Contracts;

public record ValidationError(
int RowNumber,
string PropertyName,
string Message,
ValidationSeverity Severity = ValidationSeverity.Error);

public enum ValidationSeverity
{
    Warning,
    Error,
    Critical
}
