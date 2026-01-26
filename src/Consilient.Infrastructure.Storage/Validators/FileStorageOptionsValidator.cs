using Consilient.Infrastructure.Storage.Contracts;
using Microsoft.Extensions.Options;

namespace Consilient.Infrastructure.Storage.Validators
{
    public class FileStorageOptionsValidator : IValidateOptions<FileStorageOptions>
    {
        public ValidateOptionsResult Validate(string? name, FileStorageOptions options)
        {
            var errors = new List<string>();

            if (string.Equals(options.Provider, "AzureBlob", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(options.AzureBlobConnectionString))
                    errors.Add($"{FileStorageOptions.SectionName}:AzureBlobConnectionString is required when Provider is 'AzureBlob'");
            }
            else // Local provider
            {
                if (string.IsNullOrWhiteSpace(options.LocalPath))
                    errors.Add($"{FileStorageOptions.SectionName}:LocalPath is required when Provider is 'Local'");
            }

            if (string.IsNullOrWhiteSpace(options.ContainerName))
                errors.Add($"{FileStorageOptions.SectionName}:ContainerName is required");

            return errors.Count > 0
                ? ValidateOptionsResult.Fail(errors)
                : ValidateOptionsResult.Success;
        }
    }
}
