using Consilient.Background.Workers.Contracts;
using Consilient.ProviderAssignments.Contracts.Processing;
using Consilient.ProviderAssignments.Contracts.Resolution;
using Hangfire;
using System.ComponentModel;

namespace Consilient.Background.Workers.ProviderAssignments
{
    internal class ProviderAssignmentsResolutionWorker(
        IProviderAssignmentsResolver resolver,
        IProviderAssignmentsProcessor processor) : IBackgroundWorker
    {
        [DisplayName("Resolve and Process Assignment (Batch: {0})")]
        public void Resolve([FromResult] Guid batchId, int facilityId, DateOnly date)
        {
            // Step 1: Resolve staging records
            resolver.ResolveAsync(batchId, facilityId, date, CancellationToken.None).GetAwaiter().GetResult();

            // Step 2: Process resolved records via stored procedure
            processor.ProcessAsync(batchId, CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
