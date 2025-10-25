using Serilog.Events;

namespace Consilient.Infrastructure.Logging
{
    internal static class Helpers
    {
        public static LogEventLevel ParseLogEventLevel(string input)
        {
            return Enum.Parse<LogEventLevel>(input, ignoreCase: true);
        }
    }
}
