namespace Consilient.ProviderAssignments.Contracts.Validation;

public class ValidationError
{
    public ValidationErrorType Type { get; set; }
    public string Message { get; set; } = string.Empty;

    public ValidationError()
    {
    }

    public ValidationError(ValidationErrorType type, string message)
    {
        Type = type;
        Message = message;
    }
}
