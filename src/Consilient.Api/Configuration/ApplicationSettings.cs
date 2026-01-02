namespace Consilient.Api.Configuration
{
    public class ApplicationSettings
    {
        public required FileUploadSettings FileUpload { get; init; }
        public required AuthenticationSettings Authentication { get; init; }
    }
}
