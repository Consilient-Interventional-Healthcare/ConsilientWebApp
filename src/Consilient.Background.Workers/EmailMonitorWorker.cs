
using Consilient.EmailMonitor.Contracts;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Consilient.Background.Workers
{
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public class EmailMonitorWorker(IEmailMonitor monitor, ILoggerFactory loggerFactory) : BaseRecurringWorker(loggerFactory)
    {
        private readonly IEmailMonitor Monitor = monitor;

        protected override async Task PerformJob(CancellationToken cancellationToken)
        {
            await Monitor.MonitorEmailAsync(cancellationToken);
        }
    }
}
