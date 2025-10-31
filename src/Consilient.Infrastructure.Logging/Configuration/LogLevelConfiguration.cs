namespace Consilient.Infrastructure.Logging.Configuration
{

    public class LogLevelConfiguration
    {
        public string Default { get; } = "Information";
        public string MicrosoftAspNetCore { get; } = "Warning";
    }
}
