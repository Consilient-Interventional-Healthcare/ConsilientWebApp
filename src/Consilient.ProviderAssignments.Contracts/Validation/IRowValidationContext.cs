using Consilient.Data.Entities.Staging;

namespace Consilient.ProviderAssignments.Contracts.Validation;

public interface IRowValidationContext
{
    IReadOnlyList<ValidationError> Errors { get; }
    bool HasErrors { get; }
    ProviderAssignment Row { get; }

    void AddError(ValidationErrorType type, string message);
    void PersistToRow();
    string? ToJson();
}