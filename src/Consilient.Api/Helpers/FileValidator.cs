using Consilient.Api.Configuration;

namespace Consilient.Api.Helpers
{
    public class FileValidator(ProviderAssignmentsUploadsOptions settings)
    {
        private readonly ProviderAssignmentsUploadsOptions _settings = settings;

        public FileValidationResult ValidateFile(IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return FileValidationResult.Failure("No file uploaded.");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_settings.AllowedExtensions.Contains(extension))
            {
                return FileValidationResult.Failure(
                    $"File type {extension} is not allowed. Only {string.Join(", ", _settings.AllowedExtensions)} files are accepted.");
            }

            // Validate file size
            if (file.Length > _settings.MaxFileSizeBytes)
            {
                var maxSizeMb = _settings.MaxFileSizeBytes / (1024.0 * 1024.0);
                return FileValidationResult.Failure(
                    $"File size exceeds the maximum limit of {maxSizeMb:F1}MB.");
            }

            return FileValidationResult.Success();
        }
    }
}
