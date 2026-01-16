using Consilient.Background.Workers.Contracts;
using Consilient.ProviderAssignments.Contracts;
using Hangfire;

namespace Consilient.Background.Workers.ProviderAssignments
{
    internal class ProviderAssignmentsResolutionWorker(IProviderAssignmentsResolver resolver) : IBackgroundWorker
    {
        public void Resolve([FromResult] Guid batchId)
        {
            resolver.ResolveAsync(batchId, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
