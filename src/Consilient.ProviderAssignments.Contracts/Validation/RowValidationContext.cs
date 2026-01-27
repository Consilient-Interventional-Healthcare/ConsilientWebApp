using Consilient.Data.Entities.Staging;
using Newtonsoft.Json;

namespace Consilient.ProviderAssignments.Contracts.Validation;

public class RowValidationContext
{
    private readonly List<ValidationError> _errors;

    public ProviderAssignment Row { get; }
    public IReadOnlyList<ValidationError> Errors => _errors;
    public bool HasErrors => _errors.Count > 0;

    /// <summary>
    /// Create new context for import (empty errors)
    /// </summary>
    public RowValidationContext(ProviderAssignment row)
    {
        Row = row;
        _errors = [];
    }

    /// <summary>
    /// Create context for resolution (deserialize existing errors from DB)
    /// </summary>
    public RowValidationContext(ProviderAssignment row, string? existingErrorsJson)
    {
        Row = row;
        _errors = string.IsNullOrEmpty(existingErrorsJson)
            ? []
            : JsonConvert.DeserializeObject<List<ValidationError>>(existingErrorsJson)
              ?? [];
    }

    public void AddError(ValidationErrorType type, string message)
    {
        _errors.Add(new ValidationError(type, message));
    }

    public string? ToJson()
    {
        return SerializeErrors(_errors);
    }

    /// <summary>
    /// Persist errors back to the row's JSON field
    /// </summary>
    public void PersistToRow()
    {
        Row.ValidationErrorsJson = ToJson();
    }

    /// <summary>
    /// Serialize a list of errors to JSON. Used by both import and resolution.
    /// </summary>
    public static string? SerializeErrors(IEnumerable<ValidationError> errors)
    {
        var list = errors.ToList();
        return list.Count > 0
            ? JsonConvert.SerializeObject(list)
            : null;
    }
}
