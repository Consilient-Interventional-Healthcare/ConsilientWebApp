using Consilient.Api.Infra.SignalR;
using Consilient.Background.Workers.Contracts;
using Hangfire;

namespace Consilient.Api.Infra.Hangfire;

public class WorkerJobActivator(IServiceProvider serviceProvider) : JobActivator
{
    public override object ActivateJob(Type jobType)
    {
        var job = serviceProvider.GetRequiredService(jobType);

        // Subscribe to progress events if it's a background worker
        if (job is IBackgroundWorker worker)
        {
            var listener = serviceProvider.GetRequiredService<WorkerProgressListener>();
            listener.SubscribeToWorker(worker);
        }

        return job;
    }

    public override JobActivatorScope BeginScope(JobActivatorContext context)
    {
        return new WorkerJobActivatorScope(serviceProvider.CreateScope());
    }

    private class WorkerJobActivatorScope(IServiceScope scope) : JobActivatorScope
    {
        public override object Resolve(Type type)
        {
            var job = scope.ServiceProvider.GetRequiredService(type);

            // Subscribe to progress events
            if (job is IBackgroundWorker worker)
            {
                var listener = scope.ServiceProvider.GetRequiredService<WorkerProgressListener>();
                listener.SubscribeToWorker(worker);
            }

            return job;
        }

        public override void DisposeScope()
        {
            scope.Dispose();
        }
    }
}