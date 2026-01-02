namespace Consilient.Infrastructure.Logging.Configuration
{

    public class LogLevelConfiguration
    {
        public required string Default { get; init; } = "Information";
        public required string MicrosoftAspNetCore { get; init; } = "Warning";
    }
}
