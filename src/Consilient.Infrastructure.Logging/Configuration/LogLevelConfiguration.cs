namespace Consilient.Infrastructure.Logging.Configuration
{

    public class LogLevelConfiguration
    {
        public string Default { get; set; } = "Information";
        public string MicrosoftAspNetCore { get; set; } = "Warning";
    }
}
