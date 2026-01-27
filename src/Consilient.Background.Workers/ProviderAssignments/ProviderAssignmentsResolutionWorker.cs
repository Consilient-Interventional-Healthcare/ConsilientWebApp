using Consilient.Background.Workers.Contracts;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Hangfire;
using System.ComponentModel;

namespace Consilient.Background.Workers.ProviderAssignments;

public class ProviderAssignmentsResolutionWorker(
    IProviderAssignmentsResolver resolver) : IBackgroundWorker
{
    [DisplayName("Resolve and Process Assignment (Batch: {0})")]
    public void Resolve([FromResult] Guid batchId)
    {
        // Step 1: Resolve staging records
        resolver.ResolveAsync(batchId, CancellationToken.None).GetAwaiter().GetResult();
    }
}
