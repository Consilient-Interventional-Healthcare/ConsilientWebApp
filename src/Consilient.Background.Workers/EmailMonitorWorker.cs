using Consilient.Background.Workers.Contracts;
using Consilient.Infrastructure.EmailMonitor.Contracts;
using Hangfire;

namespace Consilient.Background.Workers
{
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public class EmailMonitorWorker(IEmailMonitor monitor) : IRecurringWorker
    {
        public Task Run(CancellationToken cancellationToken = default)
        {
            return monitor.MonitorEmailAsync(cancellationToken);
        }
    }
}
