using Consilient.Infrastructure.EmailMonitor.Contracts;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Consilient.Background.Workers
{
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public class EmailMonitorWorker(IEmailMonitor monitor, ILoggerFactory loggerFactory) : BaseRecurringWorker(loggerFactory)
    {
        private readonly IEmailMonitor _monitor = monitor;

        protected override async Task PerformJob(CancellationToken cancellationToken)
        {
            await _monitor.MonitorEmailAsync(cancellationToken);
        }
    }
}
