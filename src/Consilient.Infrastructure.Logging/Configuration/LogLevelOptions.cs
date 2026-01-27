namespace Consilient.Infrastructure.Logging.Configuration;


public class LogLevelOptions
{
    public required string Default { get; init; } = "Information";
    public required string MicrosoftAspNetCore { get; init; } = "Warning";
}
