namespace Consilient.Api.Configuration
{
    public class ProviderAssignmentsUploadsOptions
    {
        public const string SectionName = "ProviderAssignmentsUploads";

        public required string UploadPath { get; init; }
        public required string[] AllowedExtensions { get; init; }
        public long MaxFileSizeBytes { get; init; }
    }
}
