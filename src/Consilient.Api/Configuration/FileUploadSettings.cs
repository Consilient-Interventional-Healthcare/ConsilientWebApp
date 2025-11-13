namespace Consilient.Api.Configuration
{
    public class FileUploadSettings
    {
        public required string UploadPath { get; init; }
        public required string[] AllowedExtensions { get; init; }
        public long MaxFileSizeBytes { get; init; }
    }
}