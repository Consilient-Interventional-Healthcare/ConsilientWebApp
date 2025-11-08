using Consilient.Data;

namespace Consilient.Api.Init
{
    internal static class HealthChecksBuilderExtensions
    {
        public static void RegisterHealthChecks(this IHealthChecksBuilder healthChecksBuilder)
        {
            healthChecksBuilder.AddDbContextCheck<ConsilientDbContext>();
        }
    }

}
