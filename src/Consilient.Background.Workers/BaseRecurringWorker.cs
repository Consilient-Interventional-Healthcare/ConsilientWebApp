using Microsoft.Extensions.Logging;

namespace Consilient.Background.Workers
{
    public abstract class BaseRecurringWorker : IBackgroundWorker
    {
        public BaseRecurringWorker(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(GetType());
        }

        protected ILogger Logger { get; private set; }

        public virtual Task Run()
        {
            return Run(CancellationToken.None);
        }

        public virtual Task Run(CancellationToken cancellationToken = default)
        {
            return PerformJob(cancellationToken);
        }

        protected abstract Task PerformJob(CancellationToken cancellationToken);


    }
}
