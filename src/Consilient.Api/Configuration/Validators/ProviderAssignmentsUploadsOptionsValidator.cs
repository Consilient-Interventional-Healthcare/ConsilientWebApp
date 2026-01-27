using Microsoft.Extensions.Options;

namespace Consilient.Api.Configuration.Validators;

public class ProviderAssignmentsUploadsOptionsValidator : IValidateOptions<ProviderAssignmentsUploadsOptions>
{
    public ValidateOptionsResult Validate(string? name, ProviderAssignmentsUploadsOptions options)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(options.UploadPath))
            errors.Add($"{ProviderAssignmentsUploadsOptions.SectionName}:UploadPath is required");

        if (options.AllowedExtensions is null || options.AllowedExtensions.Length == 0)
            errors.Add($"{ProviderAssignmentsUploadsOptions.SectionName}:AllowedExtensions must have at least one entry");

        if (options.MaxFileSizeBytes <= 0)
            errors.Add($"{ProviderAssignmentsUploadsOptions.SectionName}:MaxFileSizeBytes must be greater than 0");

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
