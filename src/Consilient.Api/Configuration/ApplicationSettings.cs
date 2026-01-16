namespace Consilient.Api.Configuration
{
    public class ApplicationSettings
    {
        public required FileUploadSettings ProviderAssignmentsUploads { get; init; }
        public required AuthenticationSettings Authentication { get; init; }
    }
}
