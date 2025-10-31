using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Extensions.Logging;

namespace Consilient.Infrastructure.Logging
{
    public static class LoggingRegistrationExtension
    {
        public static void RegisterLogging(this IServiceCollection services, ILogger logger)
        {
            services.AddSingleton(new SerilogLoggerFactory(logger));
        }
    }
}
